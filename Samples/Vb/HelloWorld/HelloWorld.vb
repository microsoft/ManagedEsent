'-----------------------------------------------------------------------
' <copyright file="HelloWorld.vb" company="Microsoft Corporation">
'     Copyright (c) Microsoft Corporation.
' </copyright>
'-----------------------------------------------------------------------

Imports System.Text
Imports Microsoft.Isam.Esent.Interop

Module HelloWorld

    Sub Main()
        Dim instance As JET_INSTANCE
        Dim sesid As JET_SESID
        Dim dbid As JET_DBID
        Dim tableid As JET_TABLEID
        Dim columnid As JET_COLUMNID

        ' Initialize ESENT
        API.JetInit(instance)
        API.JetBeginSession(instance, sesid, String.Empty, String.Empty)

        ' Create the database
        API.JetCreateDatabase(sesid, "edbtest.db", String.Empty, dbid, CreateDatabaseGrbit.OverwriteExisting)

        ' Create the table
        API.JetBeginTransaction(sesid)
        API.JetCreateTable(sesid, dbid, "table", 0, 100, tableid)
        Dim columndef As JET_COLUMNDEF = New JET_COLUMNDEF
        columndef.cp = JET_CP.ASCII
        columndef.coltyp = JET_coltyp.LongText
        API.JetAddColumn(sesid, tableid, "column", columndef, Nothing, 0, columnid)
        API.JetCommitTransaction(sesid, CommitTransactionGrbit.LazyFlush)

        ' Insert a record
        API.JetBeginTransaction(sesid)
        API.JetPrepareUpdate(sesid, tableid, JET_prep.Insert)
        Dim data() As Byte = Encoding.ASCII.GetBytes("Hello World")
        API.JetSetColumn(sesid, tableid, columnid, data, data.Length, SetColumnGrbit.None, Nothing)
        Dim bookmark(256) As Byte
        Dim bookmarkSize As Integer
        API.JetUpdate(sesid, tableid, bookmark, bookmark.Length, bookmarkSize)
        API.JetCommitTransaction(sesid, CommitTransactionGrbit.None)
        API.JetGotoBookmark(sesid, tableid, bookmark, bookmarkSize)

        ' Retrieve a column from the record
        Dim buffer(256) As Byte
        Dim retrievedSize As Integer
        API.JetRetrieveColumn(sesid, tableid, columnid, buffer, buffer.Length, retrievedSize, RetrieveColumnGrbit.None, Nothing)
        Console.WriteLine("{0}", Encoding.ASCII.GetString(buffer, 0, retrievedSize))

        ' Terminate ESENT
        API.JetCloseTable(sesid, tableid)
        API.JetEndSession(sesid, EndSessionGrbit.None)
        API.JetTerm(instance)

    End Sub

End Module
