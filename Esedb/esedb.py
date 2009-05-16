#-----------------------------------------------------------------------
# <copyright file="esedb.py" company="Microsoft Corporation">
# Copyright (c) Microsoft Corporation.
# </copyright>
#-----------------------------------------------------------------------

# TODO
#
# pop
# popitem (remove the last one)
# setdefault
# use 'with' for locks
# private names. one underscore or two?
# use str() or repr()?
# Optimization: write-lock keys in memory when updating
#	- use a hash
#	- ok to have collisions

"""Provides a simple dictionary interface to an esent database. This requires
the ManagedEsent interop dll.

>>> x = open('wdbtest.db', mode='n')
>>> x['a'] = 'somedata'
>>> x['a']
'somedata'
>>> x.has_key('b')
False
>>> x['b'] = 'somemoredata'
>>> x['c'] = 'deleteme'
>>> del x['c']
>>> x.keys()
['a', 'b']
>>> x.values()
['somedata', 'somemoredata']
>>> x.close()

"""

from __future__ import with_statement

import thread
import System
import clr

from System.IO import File, Path, Directory
from System.Text import Encoding

clr.AddReferenceByPartialName('Esent.Interop')
from Microsoft.Isam.Esent.Interop import Api

from Microsoft.Isam.Esent.Interop import JET_INSTANCE
from Microsoft.Isam.Esent.Interop import JET_SESID
from Microsoft.Isam.Esent.Interop import JET_DBID
from Microsoft.Isam.Esent.Interop import JET_TABLEID
from Microsoft.Isam.Esent.Interop import JET_COLUMNID
from Microsoft.Isam.Esent.Interop import JET_COLUMNDEF
from Microsoft.Isam.Esent.Interop import JET_CP
from Microsoft.Isam.Esent.Interop import JET_coltyp
from Microsoft.Isam.Esent.Interop import JET_param
from Microsoft.Isam.Esent.Interop import JET_prep

from Microsoft.Isam.Esent.Interop import AttachDatabaseGrbit
from Microsoft.Isam.Esent.Interop import CloseDatabaseGrbit
from Microsoft.Isam.Esent.Interop import ColumndefGrbit
from Microsoft.Isam.Esent.Interop import CommitTransactionGrbit
from Microsoft.Isam.Esent.Interop import CreateDatabaseGrbit
from Microsoft.Isam.Esent.Interop import CreateIndexGrbit
from Microsoft.Isam.Esent.Interop import EndSessionGrbit
from Microsoft.Isam.Esent.Interop import InitGrbit
from Microsoft.Isam.Esent.Interop import MakeKeyGrbit
from Microsoft.Isam.Esent.Interop import OpenDatabaseGrbit
from Microsoft.Isam.Esent.Interop import OpenTableGrbit
from Microsoft.Isam.Esent.Interop import RollbackTransactionGrbit
from Microsoft.Isam.Esent.Interop import SeekGrbit

from Microsoft.Isam.Esent.Interop import InstanceParameters
from Microsoft.Isam.Esent.Interop import SystemParameters
from Microsoft.Isam.Esent.Interop import EsentVersion

from Microsoft.Isam.Esent.Interop.Vista import VistaParam

from Microsoft.Isam.Esent.Interop.Windows7 import Windows7Param
from Microsoft.Isam.Esent.Interop.Windows7 import Windows7Grbits

#-----------------------------------------------------------------------
class _EseTransaction(object):
#-----------------------------------------------------------------------
	"""Wrapper for an esent transaction. This object can be used in
	a with statement. If the 'with' block ends normally the transaction
	will be committed, otherwise it will rollback.
	
	"""
	
	def __init__(self, sesid):
		self._sesid = sesid
		self._inTransaction = False
		
	def __enter__(self):
		self.begin()
		return self
		
	def __exit__(self, etyp, einst, etb):
		if self._inTransaction:
			if None == etyp:
				self.commit()
			else:
				# Abnormal exit, rollback the transaction
				self.rollback()
			
	def begin(self):
		assert(not self._inTransaction)
		Api.JetBeginTransaction(self._sesid)
		self._inTransaction = True
	
	def commit(self, lazyflush=False):
		assert(self._inTransaction)
		if lazyflush:
			commitgrbit = CommitTransactionGrbit.LazyFlush
		else:
			commitgrbit = CommitTransactionGrbit.None		
		Api.JetCommitTransaction(self._sesid, commitgrbit)
		self._inTransaction = False
		
	def rollback(self):
		assert(self._inTransaction)
		Api.JetRollback(self._sesid, RollbackTransactionGrbit.None)
		self._inTransaction = False			

#-----------------------------------------------------------------------
class _EseUpdate(object):
#-----------------------------------------------------------------------
	"""Wrapper for an esent update. This object can be used in
	a with statement. If the 'with' block ends normally the update
	will be done, otherwise it will be cancelled.
	
	"""
	
	def __init__(self, sesid, tableid, prep):
		self._sesid = sesid
		self._tableid = tableid
		self._prep = prep
		self._inUpdate = False
		
	def __enter__(self):
		self.prepareUpdate()
		return self
		
	def __exit__(self, etyp, einst, etb):
		if self._inUpdate:
			if None == etyp:
				# A normal exit, update the record
				self.update()
			else:
				# Abnormal exit, cancel the update
				self.cancelUpdate()
			
	def prepareUpdate(self):
		assert(not self._inUpdate)
		Api.JetPrepareUpdate(self._sesid, self._tableid, self._prep)
		self._inUpdate = True
	
	def update(self):
		assert(self._inUpdate)
		Api.JetUpdate(self._sesid, self._tableid, None, 0)
		self._inUpdate = False
		
	def cancelUpdate(self):
		assert(self._inUpdate)
		Api.JetPrepareUpdate(self._sesid, self._tableid, JET_prep.Cancel)
		self._inUpdate = False	

		
#-----------------------------------------------------------------------
class _EseDBRegistry(object):
#-----------------------------------------------------------------------
	"""EseDBCursor registry. This stores a collection of _EseDB objects
	and provides a mapping from path => EseDB. To deal with multi-threaded
	creation/deletion the object needs to be locked.
	
	There should be one global instance of this class.
		
	"""
	
	def __init__(self):
		self._databases = dict()
		self._critsec = thread.allocate_lock()
		self._instancenum = 0

	def lock(self):
		"""Lock the object. All register, unregister and lookup operations
		must be performed the the registry object locked.
		
		"""
		self._critsec.acquire()
		self.assertLocked()
		
	def unlock(self):
		"""Unlock the object."""
		self.assertLocked()
		self._critsec.release()

	def newInstanceName(self):
		"""Return a new esent instance name. This is guaranteed to be unique from
		all other esent instance names created by this function.
		
		"""
		self.assertLocked()
		instancename = 'esedb_instance_%d' % self._instancenum
		self._instancenum += 1
		return instancename
		
	def hasDB(self, filename):	
		"""Returns true if the given database is registered."""
		self.assertLocked()
		return self._databases.has_key(filename)
		
	def getDB(self, filename):
		"""Gets the database object with the given path."""
		self.assertLocked()
		return self._databases[filename]
		
	def registerDB(self, esedb):
		"""Registers the specified database object."""
		self.assertLocked()
		self._databases[esedb.filename] = esedb
		
	def unregisterDB(self, esedb):
		"""Unregisters the specified database object."""
		self.assertLocked()
		del self._databases[esedb.filename]
		
	def assertLocked(self):
		"""Assert this object is locked."""
		assert(self._critsec.locked())
						
						
#-----------------------------------------------------------------------
class _EseDB(object):
#-----------------------------------------------------------------------
	"""An esedb database. A database contains one table, which has two
	columns, providing the key => value mappings. This class contains
	a JET_INSTANCE and will open/create the database.
	
	Insert/Delete/Update/Lookup functionality is provided by the 
	EseDBCursor class. One database can have many cursors and the
	database is automatically closed when the last cursor is closed.
	
	The EseDB contains a lock used to synchronize updates by 
	multiple cursors. This isn't really needed, as esent supports highly 
	concurrent access to data, but if we allow multiple threads to update the 
	database at the same time then we will face the problem of two threads 
	updating the same record at the same time, which will generate a 
	write-conflict error. Instead of dealing with those complexities we 
	simply restrict updates to one thread at a time. For read operations the
	snapshot isolation provided by esent transactions is sufficient.
	
	"""
	
	def __init__(self, instancename, filename):
		self._filename = filename
		self._directory = Path.GetDirectoryName(filename)
		self._instancename = instancename
		self._datatable = 'esedb_data'
		self._keycolumn = 'key'
		self._valuecolumn = 'value'
		self._numCursors = 0
		self._critsec = thread.allocate_lock()
		self._instance = None	
		self._basename = 'wdb'
		
		# Cached number of records
		self.numRecords = None
		
	def openCursor(self, mode, lazyupdate):
		"""Creates a new cursor on the database. This function will
		initialize esent and create the database if necessary.
		
		This routine is synchronized by the global registry object.
		Cursors are opened while the registry is locked.
		
		"""
		_registry.assertLocked()
		readonly = False
		create = False
		if mode == 'r':
			readonly = True
		if mode == 'c' and not File.Exists(self._filename):
			create = True
		if mode == 'n':
			self._deleteDatabaseAndLogfiles()
			create = True			
					
		if None == self._instance:
			self._instance = self._createInstance()	
			grbit = InitGrbit.None
			if EsentVersion.SupportsWindows7Features:
				grbit = Windows7Grbits.ReplayIgnoreLostLogs
			Api.JetInit2(self._instance, grbit)
			
		if create:
			try:
				self._createDatabase()
			except:
				# Don't leave a partially created database lying around
				Api.JetTerm(self._instance)
				self._deleteDatabaseAndLogfiles()
				raise
				
		cursor = self._createCursor(readonly, lazyupdate)
		return cursor
		
	def closeCursor(self, esedbCursor):
		_registry.lock()
		try:
			self._numCursors -= 1
			if 0 == self._numCursors:
				# The last cursor on the database has been closed
				# unregister this object and terminate esent
				_registry.unregisterDB(self)
				Api.JetTerm(self._instance)
				self._instance = None
		finally:
			_registry.unlock()
					
	def getWriteLock(self):
		"""Gets a write-lock on the database"""
		self._critsec.acquire()
		
	def unlock(self):
		"""Releases the database lock."""
		self._critsec.release()
		
	def _createInstance(self):
		"""Create the JET_INSTANCE and set the system parameters. The
		important changes here are to reduce the logfile size, turn
		on circular logging and turn off the temporary database.
		
		"""
		instance = Api.JetCreateInstance(self._instancename)
		parameters = InstanceParameters(instance)
		parameters.SystemDirectory = self._directory
		parameters.TempDirectory = self._directory
		parameters.LogFileDirectory = self._directory
		parameters.BaseName = self._basename
		parameters.CircularLog = True
		parameters.NoInformationEvent = True
		parameters.CreatePathIfNotExist = True
		parameters.LogFileSize = 1024
		parameters.MaxTemporaryTables = 0
		
		if EsentVersion.SupportsWindows7Features:
			Api.JetSetSystemParameter(instance, JET_SESID.Nil, Windows7Param.WaypointLatency, 1, None)			
			
		return instance

	def _deleteDatabaseAndLogfiles(self):
		"""Delete the database and logfiles."""
		if File.Exists(self._filename):
			File.Delete(self._filename)
		self._deleteFilesMatching(self._directory, '%s*.log' % self._basename)
		self._deleteFilesMatching(self._directory, '%s.chk' % self._basename)
			
	def _deleteFilesMatching(self, directory, pattern):
		"""Delete files in the directory matching the pattern."""
		if Directory.Exists(directory):
			files = Directory.GetFiles(directory, pattern)
			for f in files:
				File.Delete(f)
		
	def _createDatabase(self):
		"""Create the database, table and columns."""
		sesid = Api.JetBeginSession(self._instance, '', '')		
		try:
			dbid = Api.JetCreateDatabase(
				sesid,
				self._filename,
				'',
				CreateDatabaseGrbit.OverwriteExisting)
			with _EseTransaction(sesid) as trx:
				tableid = Api.JetCreateTable(
					sesid,
					dbid,
					self._datatable,
					32,
					100)
				self._addTextColumn(sesid, tableid, self._keycolumn)
				self._addTextColumn(sesid, tableid, self._valuecolumn)
				indexdef = "+%s\0\0" % self._keycolumn
				Api.JetCreateIndex(
					sesid,
					tableid,
					'primary',
					CreateIndexGrbit.IndexUnique | CreateIndexGrbit.IndexPrimary,
					indexdef,
					indexdef.Length,
					100)
				Api.JetCloseTable(sesid, tableid)
				trx.commit(lazyflush=True)
			Api.JetCloseDatabase(sesid, dbid, CloseDatabaseGrbit.None)
			Api.JetDetachDatabase(sesid, self._filename)
			
			# As the database is newly created we know there are no records
			self.numRecords = 0
		finally:
			Api.JetEndSession(sesid, EndSessionGrbit.None)

	def _addTextColumn(self, sesid, tableid, column):
		"""Add a new text column to the given table."""
		grbit = ColumndefGrbit.None
		if EsentVersion.SupportsWindows7Features:
			grbit = Windows7Grbits.ColumnCompressed		
		columndef = JET_COLUMNDEF(
			cp = JET_CP.Unicode,
			coltyp = JET_coltyp.LongText,
			grbit = grbit)
		Api.JetAddColumn(
			sesid,
			tableid,
			column,
			columndef,
			None,
			0)
			
	def _createCursor(self, readonly, lazyupdate):
		"""Creates a new EseDBCursor."""
		sesid = Api.JetBeginSession(self._instance, '', '')
		if readonly:
			grbit = AttachDatabaseGrbit.ReadOnly
		else:
			grbit = AttachDatabaseGrbit.None
		Api.JetAttachDatabase(sesid, self._filename, grbit)
		if readonly:
			grbit = OpenDatabaseGrbit.ReadOnly
		else:
			grbit = OpenDatabaseGrbit.None
		(wrn, dbid) = Api.JetOpenDatabase(sesid, self._filename, '', grbit)
		tableid = Api.JetOpenTable(
			sesid,
			dbid,
			self._datatable,
			None,
			0,
			OpenTableGrbit.None)
		keycolumnid = self._getColumnid(sesid, tableid, self._keycolumn)
		valuecolumnid = self._getColumnid(sesid, tableid, self._valuecolumn)
		cursor = EseDBCursor(self, sesid, tableid, lazyupdate, keycolumnid, valuecolumnid)
		self._numCursors += 1
		return cursor

	def _getColumnid(self, sesid, tableid, column):
		"""Returns the columnid of the column."""
		columndef = clr.Reference[JET_COLUMNDEF]()
		Api.JetGetTableColumnInfo(
			sesid,
			tableid,
			column,
			columndef)
		return columndef.Value.columnid
		
	def _filename(self):
		"""Returns the full path of the database"""
		return self._filename
		
	filename = property(_filename, doc='full path of the database')


#-----------------------------------------------------------------------
class EseDBError(Exception):
#-----------------------------------------------------------------------
	"""Esedb exception"""
	
	def __init__(self, message):
		self._message = message
		
	def __repr__(self):
		return 'EseDBError(%s)' % self._message

	__str__ = __repr__
	
#-----------------------------------------------------------------------
class EseDBCursorClosedError(EseDBError):
#-----------------------------------------------------------------------
	"""Raised when a method is called on a closed cursor."""
	
	def __init__(self):
		EseDBError.__init__(self, 'cursor is closed')
	
	
#-----------------------------------------------------------------------
class EseDBCursor(object):
#-----------------------------------------------------------------------
	"""A cursor on an esedb database. A cursor contains a JET_SESID and
	a JET_TABLEID along with a reference to the underlying EseDB.
	
	"""
	
	# Decorator that checks self (args[0]) isn't closed
	def cursorMustBeOpen(func):
		def checked_func(*args, **kwargs):
			args[0]._checkNotClosed()
			return func(*args, **kwargs)
		# Promote the documentation so doctest will work
		checked_func.__doc__ = func.__doc__
		return checked_func

	def __init__(self, database, sesid, tableid, lazyupdate, keycolumnid, valuecolumnid):
		"""Initialize a new EseDBCursor on the specified database."""
		self._database = database
		self._sesid = sesid
		self._tableid = tableid
		self._lazyupdate = lazyupdate
		self._keycolumnid = keycolumnid
		self._valuecolumnid = valuecolumnid
		self._isopen = True
		
	def __del__(self):
		"""Called when garbage collection is removing the object. Close it."""
		self.close()
		
	@cursorMustBeOpen
	def __getitem__(self, key): 
		"""Returns the value of the record with the specified key.

		>>> x = open('wdbtest.db', mode='n')
		>>> x['a'] = 'somedata'
		>>> x['a']
		'somedata'
		>>> x.close()

		If the key isn't present in the database then a KeyError
		is raised.

		>>> x = open('wdbtest.db', mode='n')
		>>> x['a']
		Traceback (most recent call last):
		...
		KeyError: key 'a' was not found
		>>> x.close()
		
		"""
		with _EseTransaction(self._sesid):
			self._seekForKey(key)
			return self._retrieveCurrentRecordValue()

	@cursorMustBeOpen
	def __setitem__(self, key, value): 
		"""Sets the value of the record with the specified key.
		
		>>> x = open('wdbtest.db', mode='n')
		>>> x['key'] = 'value'
		>>> x.close()				
		
		"""
		self._database.getWriteLock()
		try:
			with _EseTransaction(self._sesid) as trx:
				if self.has_key(key):
					self._updateItem(key, value)
				else:
					self._insertItem(key, value)
				trx.commit(self._lazyupdate)
		finally:
			self._database.unlock()
			
	@cursorMustBeOpen
	def __delitem__(self, key): 
		"""Deletes the record with the specified key.

		>>> x = open('wdbtest.db', mode='n')
		>>> x['key'] = 'value'
		>>> del x['key']
		>>> x.close()

		If the key isn't present in the database then a KeyError
		is raised.
		
		>>> x = open('wdbtest.db', mode='n')
		>>> del x['a']
		Traceback (most recent call last):
		...
		KeyError: key 'a' was not found
		>>> x.close()
		
		"""
		self._database.getWriteLock()
		try:
			with _EseTransaction(self._sesid) as trx:
				self._seekForKey(key)
				self._deleteCurrentRecord()
				trx.commit(self._lazyupdate)
		finally:
			self._database.unlock()

	@cursorMustBeOpen
	def __len__(self):
		"""Returns the number of records in the database.
		
		>>> x = open('wdbtest.db', mode='n')
		>>> len(x)
		0
		>>> x['foo'] = 'bar'
		>>> len(x)
		1
		>>> x.close()
		
		"""
		# If there is no cached length we have to scan the database
		if None == self._database.numRecords:
			self._database.getWriteLock()
			if None == self._database.numRecords:
				try:
					with _EseTransaction(self._sesid) as trx:
						if Api.TryMoveFirst(self._sesid, self._tableid):
							self._database.numRecords = Api.JetIndexRecordCount(self._sesid, self._tableid, 0)
						else:
							self._database.numRecords = 0
				finally:
					self._database.unlock()
		return self._database.numRecords
			
	@cursorMustBeOpen
	def __contains__(self, key):
		"""Returns True if the database contains the specified key,
		otherwise returns False.

		>>> x = open('wdbtest.db', mode='n')
		>>> x['foo'] = 'bar'
		>>> 'foo' in x
		True
		>>> 'baz' in x
		False
		>>> 'baz' not in x
		True
		>>> x.close()
			
		"""
		return self.has_key(key)
		
	def close(self):
		"""Close the database. After being closed this object can no
		longer be used

		>>> x = open('wdbtest.db', mode='n')
		>>> x.close()				
		>>> x.has_key('a')
		Traceback (most recent call last):
		...
		EseDBCursorClosedError: EseDBError(cursor is closed)
		
		"""
		if self._isopen:
			Api.JetCloseTable(self._sesid, self._tableid)
			self._tableid = None
			Api.JetEndSession(self._sesid, EndSessionGrbit.None)
			self._sesid = None
			# Tell the database this cursor has been closed. The database is
			# refcounted and closing the last cursor will close the database.
			self._database.closeCursor(self)
			self._isopen = False
		
	@cursorMustBeOpen
	def clear(self):
		"""Removes all records from the database.

		>>> x = open('wdbtest.db', mode='n')
		>>> x['key'] = 'value'
		>>> x['anotherkey'] = 'anothervalue'
		>>> x.clear()
		>>> len(x)
		0
		>>> 'key' in x
		False
		>>> x.close()
		
		"""
	
		# clear() can be optimized by just deleting and
		# recreating the table
		self._database.getWriteLock()
		try:
			n = 0
			# Do deletes in batches to improve performance
			with _EseTransaction(self._sesid) as trx:
				Api.MoveBeforeFirst(self._sesid, self._tableid)
				while Api.TryMoveNext(self._sesid, self._tableid):
					n += 1
					if 0 == (n%100):
						trx.commit(lazyflush=True)
						trx.begin()					
					self._deleteCurrentRecord()
		finally:
			self._database.unlock()	
		
	@cursorMustBeOpen
	def iterkeys(self):
		"""Returns each key contained in the database. These
		are returned in sorted order.

		>>> x = open('wdbtest.db', mode='n')
		>>> x['c'] = 64
		>>> x['b'] = 128
		>>> x['a'] = 256
		>>> for k in x.iterkeys():
		...		print k	
		...		
		a
		b
		c
		>>> x.close()
		
		"""
		return self._iterateAndYield(self._retrieveCurrentRecordKey)
			
	@cursorMustBeOpen
	def keys(self):
		"""Returns a list of all keys in the database. The
		list is in sorted order.

		>>> x = open('wdbtest.db', mode='n')
		>>> x['c'] = 64
		>>> x['b'] = 128
		>>> x['a'] = 256
		>>> x.keys()
		['a', 'b', 'c']
		>>> x.close()				
	
		"""
		return list(self.iterkeys())

	@cursorMustBeOpen
	def itervalues(self):
		"""Returns each value contained in the database. These
		are returned in key order.

		>>> x = open('wdbtest.db', mode='n')
		>>> x['c'] = 64
		>>> x['b'] = 128
		>>> x['a'] = 256
		>>> for k in x.itervalues():
		...		print k	
		...		
		256
		128
		64
		>>> x.close()
		
		"""
		return self._iterateAndYield(self._retrieveCurrentRecordValue)
		
	@cursorMustBeOpen
	def values(self):
		"""Returns a list of all values in the database. The
		values are returned in key order.

		>>> x = open('wdbtest.db', mode='n')
		>>> x['c'] = 64
		>>> x['b'] = 128
		>>> x['a'] = 256
		>>> x.values()
		['256', '128', '64']
		>>> x.close()	
		
		"""
		return list(self.itervalues())
			
	@cursorMustBeOpen
	def iteritems(self):
		"""Return each key/value pair contained in the database. These
		are returned in key order.
		
		>>> x = open('wdbtest.db', mode='n')
		>>> x['c'] = 64
		>>> x['b'] = 128
		>>> x['a'] = 256
		>>> for (k,v) in x:
		...		print '%s => %s' % (k,v)	
		...		
		a => 256
		b => 128
		c => 64
		>>> x.close()

		"""
		return self._iterateAndYield(self._retrieveCurrentRecord)
			
	__iter__ = iteritems

	@cursorMustBeOpen
	def items(self):
		"""Returns a list of all items in the database as a list of
		(key, value) tuples. The items are returned in key order.

		>>> x = open('wdbtest.db', mode='n')
		>>> x['c'] = 64
		>>> x['b'] = 128
		>>> x['a'] = 256
		>>> x.items()
		[('a', '256'), ('b', '128'), ('c', '64')]
		>>> x.close()	
				
		"""
		return list(self.iteritems())
			
	@cursorMustBeOpen
	def has_key(self, key):
		"""Returns True if the database contains the specified key,
		otherwise returns False.

		>>> x = open('wdbtest.db', mode='n')
		>>> x['key'] = 'value'
		>>> x.has_key('key')
		True
		>>> x.has_key('not_a_key')
		False
		>>> x.close()
			
		"""
		with _EseTransaction(self._sesid):				
			self._makeKey(key)
			return Api.TrySeek(self._sesid, self._tableid, SeekGrbit.SeekEQ)
				
	@cursorMustBeOpen
	def set_location(self, key):
		"""Sets the cursor to the record specified by the key and returns
		a pair (key, value) for the record.
		
		>>> x = open('wdbtest.db', mode='n')
		>>> x['key'] = 'value'
		>>> x.set_location('key')
		('key', 'value')
		>>> x.close()		
		
		If the key doesn't exist in the database then the location is set
		to the next highest key and that record is returned.
		
		>>> x = open('wdbtest.db', mode='n')
		>>> x['b'] = 'value'
		>>> x.set_location('a')
		('b', 'value')
		>>> x.close()				
		
		If no matching key is found then KeyError is raised.

		>>> x = open('wdbtest.db', mode='n')
		>>> x['a'] = 'value'
		>>> x.set_location('b')
		Traceback (most recent call last):
		...
		KeyError: no key matching 'b' was found
		>>> x.close()				
		
		"""
		with _EseTransaction(self._sesid):		
			self._makeKey(key)
			if not Api.TrySeek(self._sesid, self._tableid, SeekGrbit.SeekGE):
				raise KeyError('no key matching \'%s\' was found' % key)
			return self._retrieveCurrentRecord()

	@cursorMustBeOpen
	def first(self):
		"""Sets the cursor to the first record in the database and returns
		a (key, value) for the record.
		
		>>> x = open('wdbtest.db', mode='n')
		>>> x['b'] = 128
		>>> x['a'] = 256
		>>> x.first()
		('a', '256')
		>>> x.close()			
		
		If the database is empty a KeyError is raised.

		>>> x = open('wdbtest.db', mode='n')
		>>> x.first()
		Traceback (most recent call last):
		...
		KeyError: database is empty
		>>> x.close()			
		
		"""
		with _EseTransaction(self._sesid):		
			if not Api.TryMoveFirst(self._sesid, self._tableid):
				raise KeyError('database is empty')	
			return self._retrieveCurrentRecord()
	
	@cursorMustBeOpen
	def last(self):
		"""Sets the cursor to the last record in the database and returns
		a (key, value) for the record.

		>>> x = open('wdbtest.db', mode='n')
		>>> x['c'] = 64
		>>> x['b'] = 128
		>>> x.last()
		('c', '64')
		>>> x.close()			

		If the database is empty a KeyError is raised.

		>>> x = open('wdbtest.db', mode='n')
		>>> x.last()
		Traceback (most recent call last):
		...
		KeyError: database is empty
		>>> x.close()			
		
		"""
		with _EseTransaction(self._sesid):		
			if not Api.TryMoveLast(self._sesid, self._tableid):
				raise KeyError('database is empty')		
			return self._retrieveCurrentRecord()

	@cursorMustBeOpen
	def next(self):
		"""Sets the cursor to the next record in the database and returns
		a (key, value) for the record.

		>>> x = open('wdbtest.db', mode='n')
		>>> x['b'] = 128
		>>> x['a'] = 256
		>>> x.first()
		('a', '256')
		>>> x.next()
		('b', '128')
		>>> x.close()			

		A KeyError is raised when the end of the table is reached or if 
		the table is empty.
	
		"""
		with _EseTransaction(self._sesid):		
			if not Api.TryMoveNext(self._sesid, self._tableid):
				raise KeyError('end of database')		
			return self._retrieveCurrentRecord()
		
	@cursorMustBeOpen
	def previous(self):
		"""Sets the cursor to the previous item in the database and returns
		a (key, value) for the record.
		
		>>> x = open('wdbtest.db', mode='n')
		>>> x['c'] = 64
		>>> x['b'] = 128
		>>> x.last()
		('c', '64')
		>>> x.previous()
		('b', '128')
		>>> x.close()			

		A KeyError is raised when the end of the table is reached or if 
		the table is empty.
		
		"""
		with _EseTransaction(self._sesid):		
			if not Api.TryMovePrevious(self._sesid, self._tableid):
				raise KeyError('end of database')		
			return self._retrieveCurrentRecord()		

	def _checkNotClosed(self):
		if not self._isopen:
			raise EseDBCursorClosedError()

	def _iterateAndYield(self, f):
		"""Iterate over all the records and yield the result
		of calling f() each time.
		
		The iteration is done inside of a transaction, but
		the yield happens outside of the transaction. This 
		is OK because it is always possible to move off a
		deleted record (if we fall off the end of the table
		then the iteration terminates).
		
		"""
		with _EseTransaction(self._sesid) as trx:
			Api.MoveBeforeFirst(self._sesid, self._tableid)
			while Api.TryMoveNext(self._sesid, self._tableid):
				value = f()
				trx.commit()
				yield value
				self._checkNotClosed()
				trx.begin()
			
	def _updateItem(self, key, value):
		"""Update the given key with the specified value. The key must
		exist and the cursor should already be in a transaction.
		
		"""
		self._seekForKey(key)
		with _EseUpdate(self._sesid, self._tableid, JET_prep.Replace) as u:
			self._setValueColumn(value)
			u.update()

	def _insertItem(self, key, value):
		"""Update the given key with the specified value. The key must
		not exist and the cursor should already be in a transaction.
		
		"""
		with _EseUpdate(self._sesid, self._tableid, JET_prep.Insert) as u:
			self._setKeyColumn(key)
			self._setValueColumn(value)
			u.update()
			self._incrementCachedRecordCount()

	def _deleteCurrentRecord(self):
		Api.JetDelete(self._sesid, self._tableid)
		self._decrementCachedRecordCount()	
			
	def _retrieveCurrentRecord(self):
		"""Returns a tuple of (key, value) for the current record."""
		return (self._retrieveCurrentRecordKey(), self._retrieveCurrentRecordValue())
		
	def _retrieveCurrentRecordKey(self):
		"""Gets the key of the current record."""
		return Api.RetrieveColumnAsString(self._sesid, self._tableid, self._keycolumnid)

	def _retrieveCurrentRecordValue(self):
		"""Gets the value of the current record."""
		return Api.RetrieveColumnAsString(self._sesid, self._tableid, self._valuecolumnid)
		
	def _setKeyColumn(self, key):
		"""Sets the key column. An update should be prepared."""
		Api.SetColumn(self._sesid, self._tableid, self._keycolumnid, str(key), Encoding.Unicode)

	def _setValueColumn(self, value):
		"""Sets the value column. An update should be prepared."""
		# Here we want to store None as a null column, instead of the string 'None'
		# This is different than the key column, which we store a 'None' (to avoid 
		# null keys in the database).
		if None == value:
			data = None
		else:
			data = str(value)		
		Api.SetColumn(self._sesid, self._tableid, self._valuecolumnid, data, Encoding.Unicode)
				
	def _makeKey(self, key):
		"""Construct a key for the given value."""
		Api.MakeKey(self._sesid, self._tableid, str(key), Encoding.Unicode, MakeKeyGrbit.NewKey)

	def _seekForKey(self, key):
		"""Seek for the specified key. A KeyError exception is raised if the
		key isn't found.
		
		"""
		self._makeKey(key)
		if not Api.TrySeek(self._sesid, self._tableid, SeekGrbit.SeekEQ):
			raise KeyError('key \'%s\' was not found' % key)

	def _decrementCachedRecordCount(self):
		if None <> self._database.numRecords:
			assert(self._database.numRecords > 0)
			self._database.numRecords -= 1
			assert(self._database.numRecords >= 0)

	def _incrementCachedRecordCount(self):
		if None <> self._database.numRecords:
			assert(self._database.numRecords >= 0)
			self._database.numRecords += 1
			assert(self._database.numRecords > 0)
	
			
#-----------------------------------------------------------------------
def open(filename, mode='c', lazyupdate=False):
#-----------------------------------------------------------------------
	"""Open an esent database and return an EseDBCursor object. Filename is
	the path to the database, including the extension. Mode specifies
	the mode to use. 'r' opens the database read-only. 'w' opens the
	databases read-write. 'c' opens the database read-write, creating it
	if necessary, and 'n' always creates a new, empty database.
	
	As well as the database file, this will create transaction logs and
	a checkpoint file in the same directory as the database (if read/write
	access is requested). The logs and checkpoint will start with a prefix
	of 'wdb'.
	
	If lazyupdate is true, then the transaction logs will be written in
	a lazy fashion. This will preserve database consistency, but some data
	will be lost if there is an unexpected shutdown (crash).	
	
	"""
	filename = Path.GetFullPath(filename)
	
	if not mode in 'rwcn':
		raise EseDBError('invalid mode')
	
	_registry.lock()
	try:
		if not _registry.hasDB(filename):
			instancename = _registry.newInstanceName()
			newDB = _EseDB(instancename, filename)
			_registry.registerDB(newDB)
		db = _registry.getDB(filename)
		return db.openCursor(mode, lazyupdate)				
	finally:
		_registry.unlock()			

		
# Set global esent options
SystemParameters.DatabasePageSize = 8192

# Turn on small-config, if available
if EsentVersion.SupportsVistaFeatures:
	Api.JetSetSystemParameter(JET_INSTANCE.Nil, JET_SESID.Nil, VistaParam.Configuration, 0, None)
	Api.JetSetSystemParameter(JET_INSTANCE.Nil, JET_SESID.Nil, VistaParam.EnableAdvanced, 1, None)
	SystemParameters.CacheSizeMin = 64
	SystemParameters.CacheSizeMax = 2**30

# A global object to perform filename => EseDB mappings
_registry = _EseDBRegistry()

if __name__ == '__main__':
	# Doctest tests. This is a list of methods that have doctest
	# tests. Doctest isn't finding them by default.
	__test__ = dict()
	__test__['__getitem__'] = EseDBCursor.__getitem__
	__test__['__setitem__'] = EseDBCursor.__setitem__
	__test__['__delitem__'] = EseDBCursor.__delitem__
	__test__['__contains__'] = EseDBCursor.__contains__
	__test__['__len__'] = EseDBCursor.__len__
	__test__['close'] = EseDBCursor.close
	__test__['iterkeys'] = EseDBCursor.iterkeys
	__test__['keys'] = EseDBCursor.keys
	__test__['itervalues'] = EseDBCursor.itervalues
	__test__['values'] = EseDBCursor.values
	__test__['iteritems'] = EseDBCursor.iteritems
	__test__['items'] = EseDBCursor.items
	__test__['has_key'] = EseDBCursor.has_key
	__test__['set_location'] = EseDBCursor.set_location
	__test__['first'] = EseDBCursor.first
	__test__['last'] = EseDBCursor.last
	__test__['next'] = EseDBCursor.next
	__test__['previous'] = EseDBCursor.previous
	import doctest
	doctest.testmod()
	