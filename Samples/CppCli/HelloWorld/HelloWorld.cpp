//-----------------------------------------------------------------------
// <copyright file="HelloWorld.cpp" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using namespace System;
using namespace System::Text;
using namespace Microsoft::Isam::Esent::Interop;

int main(array<System::String ^> ^args)
{
	JET_INSTANCE instance;
	JET_SESID sesid;
	JET_DBID dbid;
	JET_TABLEID tableid;
	JET_COLUMNID columnid;

	// Initialize ESENT
	API::JetInit(instance);
	API::JetBeginSession(instance, sesid, String::Empty, String::Empty);

	// Create the database
	API::JetCreateDatabase(sesid, "edbtest.db", String::Empty, dbid, CreateDatabaseGrbit::OverwriteExisting);

	// Create the table
	API::JetBeginTransaction(sesid);
	API::JetCreateTable(sesid, dbid, "table", 0, 100, tableid);
	JET_COLUMNDEF^ columndef = gcnew JET_COLUMNDEF();
	columndef->cp = JET_CP::ASCII;
	columndef->coltyp = JET_coltyp::LongText;
	API::JetAddColumn(sesid, tableid, "column", columndef, nullptr, 0, columnid);
	API::JetCommitTransaction(sesid, CommitTransactionGrbit::LazyFlush);

	// Insert a record
	API::JetBeginTransaction(sesid);
	API::JetPrepareUpdate(sesid, tableid, JET_prep::Insert);
	array<Byte>^ data = Encoding::ASCII->GetBytes(L"Hello World");
	API::JetSetColumn(sesid, tableid, columnid, data, data->Length, SetColumnGrbit::None, nullptr);
	array<Byte>^ bookmark = gcnew array<Byte>(256);
	int bookmarkSize;
	API::JetUpdate(sesid, tableid, bookmark, bookmark->Length, bookmarkSize);
	API::JetCommitTransaction(sesid, CommitTransactionGrbit::None);
	API::JetGotoBookmark(sesid, tableid, bookmark, bookmarkSize);

	// Retrieve a column from the record
	array<Byte>^ buffer = gcnew array<Byte>(1024);
	int retrievedSize;
	API::JetRetrieveColumn(sesid, tableid, columnid, buffer, buffer->Length, retrievedSize, RetrieveColumnGrbit::None, nullptr);
	Console::WriteLine("{0}", Encoding::ASCII->GetString(buffer, 0, retrievedSize));

	// Terminate ESENT
	API::JetCloseTable(sesid, tableid);
	API::JetEndSession(sesid, EndSessionGrbit::None);
	API::JetTerm(instance);

	return 0;
}
