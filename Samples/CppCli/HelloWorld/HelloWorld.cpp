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
	Api::JetInit(instance);
	Api::JetBeginSession(instance, sesid, String::Empty, String::Empty);

	// Create the database
	Api::JetCreateDatabase(sesid, "edbtest.db", String::Empty, dbid, CreateDatabaseGrbit::OverwriteExisting);

	// Create the table
	Api::JetBeginTransaction(sesid);
	Api::JetCreateTable(sesid, dbid, "table", 0, 100, tableid);
	JET_COLUMNDEF^ columndef = gcnew JET_COLUMNDEF();
	columndef->cp = JET_CP::ASCII;
	columndef->coltyp = JET_coltyp::LongText;
	Api::JetAddColumn(sesid, tableid, "column", columndef, nullptr, 0, columnid);
	Api::JetCommitTransaction(sesid, CommitTransactionGrbit::LazyFlush);

	// Insert a record
	Api::JetBeginTransaction(sesid);
	Api::JetPrepareUpdate(sesid, tableid, JET_prep::Insert);
	array<Byte>^ data = Encoding::ASCII->GetBytes(L"Hello World");
	Api::JetSetColumn(sesid, tableid, columnid, data, data->Length, SetColumnGrbit::None, nullptr);
	array<Byte>^ bookmark = gcnew array<Byte>(256);
	int bookmarkSize;
	Api::JetUpdate(sesid, tableid, bookmark, bookmark->Length, bookmarkSize);
	Api::JetCommitTransaction(sesid, CommitTransactionGrbit::None);
	Api::JetGotoBookmark(sesid, tableid, bookmark, bookmarkSize);

	// Retrieve a column from the record
	array<Byte>^ buffer = gcnew array<Byte>(1024);
	int retrievedSize;
	Api::JetRetrieveColumn(sesid, tableid, columnid, buffer, buffer->Length, retrievedSize, RetrieveColumnGrbit::None, nullptr);
	Console::WriteLine("{0}", Encoding::ASCII->GetString(buffer, 0, retrievedSize));

	// Terminate ESENT
	Api::JetCloseTable(sesid, tableid);
	Api::JetEndSession(sesid, EndSessionGrbit::None);
	Api::JetTerm(instance);

	return 0;
}
