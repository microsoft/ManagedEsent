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
        Api.JetInit(instance)
        Api.JetBeginSession(instance, sesid, String.Empty, String.Empty)

        ' Create the database
        Api.JetCreateDatabase(sesid, "edbtest.db", String.Empty, dbid, CreateDatabaseGrbit.OverwriteExisting)

        ' Create the table
        Api.JetBeginTransaction(sesid)
        Api.JetCreateTable(sesid, dbid, "table", 0, 100, tableid)
        Dim columndef As JET_COLUMNDEF = New JET_COLUMNDEF
        columndef.cp = JET_CP.ASCII
        columndef.coltyp = JET_coltyp.LongText
        Api.JetAddColumn(sesid, tableid, "column", columndef, Nothing, 0, columnid)
        Api.JetCommitTransaction(sesid, CommitTransactionGrbit.LazyFlush)

        ' Insert a record
        Api.JetBeginTransaction(sesid)
        Api.JetPrepareUpdate(sesid, tableid, JET_prep.Insert)
        Dim data() As Byte = Encoding.ASCII.GetBytes("Hello World")
        Api.JetSetColumn(sesid, tableid, columnid, data, data.Length, SetColumnGrbit.None, Nothing)
        Dim bookmark(256) As Byte
        Dim bookmarkSize As Integer
        Api.JetUpdate(sesid, tableid, bookmark, bookmark.Length, bookmarkSize)
        Api.JetCommitTransaction(sesid, CommitTransactionGrbit.None)
        Api.JetGotoBookmark(sesid, tableid, bookmark, bookmarkSize)

        ' Retrieve a column from the record
        Dim buffer(256) As Byte
        Dim retrievedSize As Integer
        Api.JetRetrieveColumn(sesid, tableid, columnid, buffer, buffer.Length, retrievedSize, RetrieveColumnGrbit.None, Nothing)
        Console.WriteLine("{0}", Encoding.ASCII.GetString(buffer, 0, retrievedSize))

        ' Terminate ESENT
        Api.JetCloseTable(sesid, tableid)
        Api.JetEndSession(sesid, EndSessionGrbit.None)
        Api.JetTerm(instance)

    End Sub

End Module
