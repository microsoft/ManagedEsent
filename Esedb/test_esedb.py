#-----------------------------------------------------------------------
# <copyright file="test_esedb.py" company="Microsoft Corporation">
# Copyright (c) Microsoft Corporation.
# </copyright>
#-----------------------------------------------------------------------

import unittest
import esedb
import System

from System.IO import Directory
from System.IO import Path
from esedb import EseDBCursorClosedError

class EsedbSingleDBFixture(unittest.TestCase):
	"""Basics tests for esedb. This fixture creates an empty database and tests
	individual operations against it.
	
	"""
	
	def setUp(self):
		self._dataDirectory = 'unittest_data'
		self._deleteDataDirectory()
		self._db = esedb.open(self._makeDatabasePath('test.edb'), lazyupdate=True)
		
	def tearDown(self):
		self._db.close()
		self._deleteDataDirectory()

	def _deleteDataDirectory(self):
		if Directory.Exists(self._dataDirectory):
			Directory.Delete(self._dataDirectory, True)
	
	def _makeDatabasePath(self, filename):
		return Path.Combine(self._dataDirectory, filename)
	
	def testInsertAndRetrieveRecord(self):
		self._db['key'] = 'value'
		self.assertEqual(self._db['key'], 'value')

	def testLargeKey(self):
		# esent will truncate the key (only the first 255 bytes
		# are indexed, but we should be able to set all this data)
		key = 'K' * 1024*1024
		self._db[key] = 'value'
		self.assertEqual(self._db[key], 'value')

	def testLargeValue(self):
		value = 'V' * 1024*1024
		self._db['bigstuff'] = value
		self.assertEqual(self._db['bigstuff'], value)
		
	def testNullKey(self):
		self._db[None] = 'value'
		self.assertEqual(self._db[None], 'value')

	def testNullValue(self):
		self._db['key'] = None
		self.assertEqual(self._db['key'], None)

	def testOverwriteRecord(self):
		self._db['key'] = 'value'
		self._db['key'] = 'newvalue'
		self.assertEqual(self._db['key'], 'newvalue')
		
	def testHasKeyReturnsFalseWhenKeyNotPresent(self):
		self.assertEqual(False, self._db.has_key('key'))

	def testHasKeyReturnsTrueWhenKeyIsPresent(self):
		self._db['key'] = 'value'
		self.assertEqual(True, self._db.has_key('key'))

	def testRetrieveRaisesKeyErrorWhenKeyNotPresent(self):
		self.assertRaises(KeyError, self._db.__getitem__, 'key')

	def testDeleteRemovesKey(self):
		self._db['key'] = 'value'
		del self._db['key']
		self.assertEqual(False, self._db.has_key('key'))

	def testDeleteRaisesKeyErrorWhenKeyNotPresent(self):
		try:
			del self._db['key']
			self.fail('expected a KeyError')
		except KeyError:
			pass
	
	def testSetLocationFindsExactMatch(self):
		self._db['key'] = 'value'
		self.assertEqual(('key', 'value'), self._db.set_location('key'))

	def testSetLocationFindsNextHighest(self):
		self._db['key'] = 'value'
		self.assertEqual(('key', 'value'), self._db.set_location('k'))

	def testSetLocationRaisesKeyErrorIfNoMatch(self):
		self._db['key'] = 'value'
		self.assertRaises(KeyError, self._db.set_location, 'x')
		
	def testFirstRaisesKeyErrorIfEmpty(self):
		self.assertRaises(KeyError, self._db.first)

	def testLastRaisesKeyErrorIfEmpty(self):
		self.assertRaises(KeyError, self._db.last)

	def testNextRaisesKeyErrorIfEmpty(self):
		self.assertRaises(KeyError, self._db.next)

	def testPreviousRaisesKeyErrorIfEmpty(self):
		self.assertRaises(KeyError, self._db.previous)
		
	def testKeysReturnsEmptyListIfEmpty(self):
		self.assertEqual([], self._db.keys())

	def testValuesReturnsEmptyListIfEmpty(self):
		self.assertEqual([], self._db.values())

	def testItemsReturnsEmptyListIfEmpty(self):
		self.assertEqual([], self._db.items())
		
		
class EsedbIterationFixture(unittest.TestCase):
	"""Iteration tests for esedb. This fixture creates a database with a
	fixed set of records and iterates over them.
	
	"""
	
	def setUp(self):
		self._dataDirectory = 'unittest_data'
		self._deleteDataDirectory()
		self._db = esedb.open(self._makeDatabasePath('test.edb'), lazyupdate=True)
		self._db['a'] = '1'
		self._db['b'] = '2'
		self._db['c'] = '3'
		self._db['d'] = '4'
		
	def tearDown(self):
		self._db.close()
		self._deleteDataDirectory()

	def _deleteDataDirectory(self):
		if Directory.Exists(self._dataDirectory):
			Directory.Delete(self._dataDirectory, True)
	
	def _makeDatabasePath(self, filename):
		return Path.Combine(self._dataDirectory, filename)
		
	def testFirstReturnsFirstRecord(self):
		self.assertEqual(('a', '1'), self._db.first())

	def testLastReturnsLastRecord(self):
		self.assertEqual(('d', '4'), self._db.last())

	def testNextReturnsNextRecord(self):
		self._db.first()
		self.assertEqual(('b', '2'), self._db.next())

	def testPreviousReturnsNextRecord(self):
		self._db.last()
		self.assertEqual(('c', '3'), self._db.previous())
		
	def testKeysReturnsKeys(self):
		self.assertEqual(['a', 'b', 'c', 'd'], self._db.keys())

	def testValuesReturnsValues(self):
		self.assertEqual(['1', '2', '3', '4'], self._db.values())

	def testItemsReturnsItems(self):
		self.assertEqual(
			[('a', '1'), ('b', '2'), ('c', '3'), ('d', '4')],
			self._db.items())

			
class EsedbFixture(unittest.TestCase):
	"""Tests for esedb."""
	
	def setUp(self):
		self._dataDirectory = 'unittest_data'
		self._deleteDataDirectory()
		
	def tearDown(self):
		self._deleteDataDirectory()

	def _deleteDataDirectory(self):
		if Directory.Exists(self._dataDirectory):
			Directory.Delete(self._dataDirectory, True)
	
	def _makeDatabasePath(self, filename):
		return Path.Combine(self._dataDirectory, filename)

	def testCloseTwice(self):
		db = esedb.open(self._makeDatabasePath('test.edb'))
		db.close()
		db.close()

	def testMultipleCursorsInsertAndDelete(self):
		db1 = esedb.open(self._makeDatabasePath('test.edb'), 'n')
		db2 = esedb.open(self._makeDatabasePath('test.edb'), 'c')
		db3 = esedb.open(self._makeDatabasePath('test.edb'), 'w')
		db_ro = esedb.open(self._makeDatabasePath('test.edb'), 'r')
		
		db1['hello'] = 'world'

		self.assertEqual('world', db1['hello'])
		self.assertEqual('world', db2['hello'])
		self.assertEqual('world', db3['hello'])
		self.assertEqual('world', db_ro['hello'])

		del db3['hello']
		
		self.assertEqual(False, db1.has_key('hello'))
		self.assertEqual(False, db2.has_key('hello'))
		self.assertEqual(False, db3.has_key('hello'))
		self.assertEqual(False, db_ro.has_key('hello'))

		db1.close()
		db2.close()		
		db3.close()		
		db_ro.close()
		
	def testMultipleCursors(self):
		db1 = esedb.open(self._makeDatabasePath('test.edb'), 'n')
		db2 = esedb.open(self._makeDatabasePath('test.edb'), 'c')
		db3 = esedb.open(self._makeDatabasePath('test.edb'), 'w')
		db_ro = esedb.open(self._makeDatabasePath('test.edb'), 'r')
				
		db1['foo'] = 123
		db2['bar'] = 456
		db3['baz'] = 789		
		db1.close()
		db3.close()
		
		self.assertEqual(['bar', 'baz', 'foo'], db2.keys())
		self.assertEqual(['456', '789', '123'], db_ro.values())		
		db2['foo'] = 'xyzzy'
		db2.close()		
		self.assertEqual('xyzzy', db_ro['foo'])		
		db_ro.close()

	def testMultipleDatabases(self):
		db1 = esedb.open(self._makeDatabasePath('db1\\test1.edb'), 'n')
		db2 = esedb.open(self._makeDatabasePath('db2\\test2.edb'), 'c')
		
		db1['hello'] = 'world'
		db2['hello'] = 'there'
		
		self.assertEqual('world', db1['hello'])
		self.assertEqual('there', db2['hello'])
		
		db1.close()
		db2.close()
		
	def testCloseAndReopenWithCreate(self):
		db = esedb.open(self._makeDatabasePath('test.edb'), 'n')
		db['jet blue'] = 'ese'
		db.close()

		db = esedb.open(self._makeDatabasePath('test.edb'), 'c')
		self.assertEqual('ese', db['jet blue'])
		db.close()

	def testCloseAndReopenForWrite(self):
		db = esedb.open(self._makeDatabasePath('test.edb'), 'n')
		db['jet blue'] = 'ese'
		db.close()

		db = esedb.open(self._makeDatabasePath('test.edb'), 'w')
		self.assertEqual('ese', db['jet blue'])
		db.close()

	def testCloseAndReopenReadOnly(self):
		db = esedb.open(self._makeDatabasePath('test.edb'), 'n')
		db['jet blue'] = 'ese'
		db.close()

		db = esedb.open(self._makeDatabasePath('test.edb'), 'r')
		self.assertEqual('ese', db['jet blue'])
		db.close()
		
	def testCloseAndOverwrite(self):
		db = esedb.open(self._makeDatabasePath('test.edb'), 'n')
		db['stuff'] = 'xxxxxx'
		db.close()

		db = esedb.open(self._makeDatabasePath('test.edb'), 'n')
		self.assertEqual(False, db.has_key('stuff'))
		db.close()
		
		
class EsedbClosedCursorFixture(unittest.TestCase):
	"""Tests for esedb on a closed cursor."""
	
	def setUp(self):
		self._dataDirectory = 'unittest_data'
		self._deleteDataDirectory()
		self._db = esedb.open(self._makeDatabasePath('test.edb'), lazyupdate=True)
		self._db.close()
		self._deleteDataDirectory()
		
	def _deleteDataDirectory(self):
		if Directory.Exists(self._dataDirectory):
			Directory.Delete(self._dataDirectory, True)
	
	def _makeDatabasePath(self, filename):
		return Path.Combine(self._dataDirectory, filename)

	def testGetitemRaisesErrorOnClosedCursor(self):
		self.assertRaises(EseDBCursorClosedError, self._db.__getitem__, '?')

	def testSetitemRaisesErrorOnClosedCursor(self):
		self.assertRaises(EseDBCursorClosedError, self._db.__setitem__, '?', '?')

	def testDelitemRaisesErrorOnClosedCursor(self):
		self.assertRaises(EseDBCursorClosedError, self._db.__delitem__, '?')

	def testKeysRaisesErrorOnClosedCursor(self):
		self.assertRaises(EseDBCursorClosedError, self._db.keys)

	def testValuesRaisesErrorOnClosedCursor(self):
		self.assertRaises(EseDBCursorClosedError, self._db.values)

	def testItemsRaisesErrorOnClosedCursor(self):
		self.assertRaises(EseDBCursorClosedError, self._db.items)

	def testSetlocationRaisesErrorOnClosedCursor(self):
		self.assertRaises(EseDBCursorClosedError, self._db.set_location, '?')

	def testHaskeyRaisesErrorOnClosedCursor(self):
		self.assertRaises(EseDBCursorClosedError, self._db.has_key, '?')

	def testFirstRaisesErrorOnClosedCursor(self):
		self.assertRaises(EseDBCursorClosedError, self._db.first)		

	def testLastRaisesErrorOnClosedCursor(self):
		self.assertRaises(EseDBCursorClosedError, self._db.last)		

	def testNextRaisesErrorOnClosedCursor(self):
		self.assertRaises(EseDBCursorClosedError, self._db.next)		

	def testPreviousRaisesErrorOnClosedCursor(self):
		self.assertRaises(EseDBCursorClosedError, self._db.previous)		
		
if __name__ == '__main__':
	unittest.main()
	