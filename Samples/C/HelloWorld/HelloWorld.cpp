//-----------------------------------------------------------------------
// <copyright file="HelloWorld.cpp" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

#include <stdio.h>
#include <string.h>
#include <esent.h>

#define Call(func) { \
	if((err = (func)) < JET_errSuccess) \
		goto HandleError; \
} \

int main(int argc, char * argv[]) {
	JET_ERR err;
	JET_INSTANCE instance;
	JET_SESID sesid;
	JET_DBID dbid;
	JET_TABLEID tableid;
	JET_COLUMNDEF columndef = {0};
	JET_COLUMNID columnid;

	char * message = "Hello World";

	/* Initialize ESENT */
	Call(JetInit(&instance));
	Call(JetBeginSession(instance, &sesid, 0, 0));

	/* Create the database, table and column */
	Call(JetCreateDatabase(sesid, "MyDatabase.db", 0, &dbid, 0));
	Call(JetBeginTransaction(sesid));
	Call(JetCreateTable(sesid, dbid, "MyTable", 0, 100, &tableid));
	columndef.cbStruct = sizeof(columndef);
	columndef.coltyp = JET_coltypLongText;
	columndef.cp = 1252;
	Call(JetAddColumn(sesid, tableid, "MyColumn", &columndef, NULL, 0, &columnid));
	Call(JetCommitTransaction(sesid, JET_bitCommitLazyFlush));

	/* Insert a record */
	Call(JetBeginTransaction(sesid));
	Call(JetPrepareUpdate(sesid, tableid, JET_prepInsert));
	Call(JetSetColumn(sesid, tableid, columnid, message, strlen(message), 0, NULL));
	Call(JetUpdate(sesid, tableid, NULL, 0, NULL));
	Call(JetCommitTransaction(sesid, 0));

	/* Retrieve the column from the first record in the table */
	char buffer[16];
	unsigned long retrievedSize;
	Call(JetMove(sesid, tableid, JET_MoveFirst, 0));
	Call(JetRetrieveColumn(sesid, tableid, columnid, buffer, sizeof(buffer), &retrievedSize, 0, NULL));
	buffer[retrievedSize] = 0;
	printf("%s\n", buffer);

    /* Terminate ESENT */
    JetCloseTable(sesid, tableid);
    JetEndSession(sesid, 0);
    JetTerm(instance);
	return 0;

HandleError:
	printf("ESENT error: %d\n", err);
	return 1; 
}
