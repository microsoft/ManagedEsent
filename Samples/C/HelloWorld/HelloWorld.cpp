//-----------------------------------------------------------------------
// <copyright file="HelloWorld.cpp" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

// We need to define JET_VERSION to be at least 0x0501 to get access to the
// JET_bitDbOverwriteExisting option and JetCreateInstance. This means this
// program will run onbWindows XP and up (Windows Server 2003 and up).
#undef JET_VERSION
#define JET_VERSION 0x0501

#include <stdio.h>
#include <string.h>
#include <esent.h>

// One possible error-handling strategy is to jump to an error-handling
// label whenever an ESENT call fails.
#define Call(func) { \
	err = (func); \
	if(err < JET_errSuccess) { \
	goto HandleError; \
	} \
} \

int main(int argc, char * argv[]) {
	JET_ERR err;
	JET_INSTANCE instance;
	JET_SESID sesid;
	JET_DBID dbid;
	JET_TABLEID tableid;

	JET_COLUMNDEF columndef = {0};
	JET_COLUMNID columnid;

	// Initialize ESENT. Setting JET_paramCircularLog to 1 means ESENT will automatically
	// delete unneeded logfiles. JetInit will inspect the logfiles to see if the last
	// shutdown was clean. If it wasn't (e.g. the application crashed) recovery will be
	// run automatically bringing the database to a consistent state.
	Call(JetCreateInstance(&instance, "instance"));
	Call(JetSetSystemParameter(&instance, JET_sesidNil, JET_paramCircularLog, 1, NULL));
	Call(JetInit(&instance));
	Call(JetBeginSession(instance, &sesid, 0, 0));

	// Create the database. To open an existing database use the JetAttachDatabase and 
	// JetOpenDatabase APIs.
	Call(JetCreateDatabase(sesid, "edbtest.db", 0, &dbid, JET_bitDbOverwriteExisting));

	// Create the table. Meta-data operations are transacted and can be performed concurrently.
	// For example, one session can add a column to a table while another session is reading
	// or updating records in the table (the newly added column won't be visible to the second session).
	// This table has no indexes defined, so it will use the default sequential index. Indexes
	// can be defined with the JetCreateIndex API.
	Call(JetBeginTransaction(sesid));
	Call(JetCreateTable(sesid, dbid, "table", 0, 100, &tableid));
	columndef.cbStruct = sizeof(columndef);
	columndef.coltyp = JET_coltypLongText;
	columndef.cp = 1252;
	Call(JetAddColumn(sesid, tableid, "column1", &columndef, NULL, 0, &columnid));
	Call(JetCommitTransaction(sesid, JET_bitCommitLazyFlush));

	// Insert a record. This table only has one column but each table can have a bit over 64,000
	// columns defined. Unless a column is declared as fixed or variable it won't take any space
	// in the record unless set. An individual record can have several hundred columns set at one
	// time, the exact number depends on the database page size and the contents of the columns.
	Call(JetBeginTransaction(sesid));
	Call(JetPrepareUpdate(sesid, tableid, JET_prepInsert));
	char * message = "Hello World";
	Call(JetSetColumn(sesid, tableid, columnid, message, strlen(message)+1, 0, NULL));
	Call(JetUpdate(sesid, tableid, NULL, 0, NULL));
	Call(JetCommitTransaction(sesid, 0));	// Use JetRollback() to abort the transaction
	
	// Retrieve a column from the record. Here we move to the first record with JetMove. By using
	// JetMoveNext it is possible to iterate through all records in a table. Use JetMakeKey and
	// JetSeek to move to a particular record.
	Call(JetMove(sesid, tableid, JET_MoveFirst, 0));
	char buffer[1024];
	Call(JetRetrieveColumn(sesid, tableid, columnid, buffer, sizeof(buffer), NULL, 0, NULL));
	printf("%s\n", buffer);

	// Terminate ESENT. This performs a clean shutdown.
	JetCloseTable(sesid, tableid);
	JetEndSession(sesid, 0);
	JetTerm(instance);
	return 0;

HandleError:
	printf("ESENT error %d\n", err);
	return 1; 
}
