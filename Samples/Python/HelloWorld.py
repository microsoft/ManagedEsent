#-----------------------------------------------------------------------
# <copyright file="HelloWorld.py" company="Microsoft Corporation">
# Copyright (c) Microsoft Corporation.
# </copyright>
#-----------------------------------------------------------------------

import System
import clr
clr.AddReferenceByPartialName('Esent.Interop')
from System import Console
from System.Text import Encoding
from Microsoft.Isam.Esent.Interop import *

# Initialize ESENT
instance = API.JetInit(JET_INSTANCE.Nil)
sesid = API.JetBeginSession(instance, '', '')

# Create the database
dbid = API.JetCreateDatabase(sesid, 'edbtest.db', '', CreateDatabaseGrbit.OverwriteExisting)

# Create the table
API.JetBeginTransaction(sesid)
tableid = API.JetCreateTable(sesid, dbid, 'table', 0, 100)
columndef = JET_COLUMNDEF(cp = JET_CP.ASCII, coltyp = JET_coltyp.LongText)
columnid = API.JetAddColumn(sesid, tableid, 'column', columndef, None, 0)
API.JetCommitTransaction(sesid, CommitTransactionGrbit.LazyFlush)

# Insert a record
API.JetBeginTransaction(sesid)
API.JetPrepareUpdate(sesid, tableid, JET_prep.Insert)
data = Encoding.ASCII.GetBytes('Hello World')
API.JetSetColumn(sesid, tableid, columnid, data, data.Length, SetColumnGrbit.None, None)
bookmark = System.Array.CreateInstance(System.Byte, 256)
bookmarkSize = API.JetUpdate(sesid, tableid, bookmark, bookmark.Length)
API.JetCommitTransaction(sesid, CommitTransactionGrbit.None)
API.JetGotoBookmark(sesid, tableid, bookmark, bookmarkSize)

# Retrieve a column from the record
buffer = System.Array.CreateInstance(System.Byte, 1024)
(wrn, retrievedSize) = API.JetRetrieveColumn(sesid, tableid, columnid, buffer, buffer.Length, RetrieveColumnGrbit.None, None)
Console.WriteLine('{0}', Encoding.ASCII.GetString(buffer, 0, retrievedSize))

# Terminate ESENT
API.JetCloseTable(sesid, tableid)
API.JetEndSession(sesid, EndSessionGrbit.None)
API.JetTerm(instance)

 

 
