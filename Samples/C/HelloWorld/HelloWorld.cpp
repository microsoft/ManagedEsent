//-----------------------------------------------------------------------
// <copyright file="HelloWorld.cpp" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

// We need to define JET_VERSION to be at least 0x0501 to get access to the
// JET_bitDbOverwriteExisting option. This means this program will run on
// Windows XP and up (Windows Server 2003 and up).
#undef JET_VERSION
#define JET_VERSION 0x0501

#include <stdio.h>
#include <string.h>
#include <esent.h>

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

	/* Initialize ESENT */
	Call(JetInit(&instance));
	Call(JetBeginSession(instance, &sesid, 0, 0));

	/* Create the database */
	Call(JetCreateDatabase(sesid, "edbtest.db", 0, &dbid, JET_bitDbOverwriteExisting));

	/* Create the table */
	Call(JetBeginTransaction(sesid));
	Call(JetCreateTable(sesid, dbid, "table", 0, 100, &tableid));
	columndef.cbStruct = sizeof(columndef);
	columndef.coltyp = JET_coltypLongText;
	columndef.cp = 1252;
	Call(JetAddColumn(sesid, tableid, "column1", &columndef, NULL, 0, &columnid));
	Call(JetCommitTransaction(sesid, JET_bitCommitLazyFlush));

	/* Insert a record */
	Call(JetBeginTransaction(sesid));
	Call(JetPrepareUpdate(sesid, tableid, JET_prepInsert));
	char * message = "Hello world";
	Call(JetSetColumn(sesid, tableid, columnid, message, strlen(message), 0, NULL));
	unsigned char bookmark[256];
	unsigned long bookmarkSize;
	Call(JetUpdate(sesid, tableid, bookmark, sizeof(bookmark), &bookmarkSize));
	Call(JetCommitTransaction(sesid, 0));
	Call(JetGotoBookmark(sesid, tableid, bookmark, bookmarkSize));

	/* Retrieve a column from the record */
	char buffer[1024];
	unsigned long retrievedSize;
	Call(JetMove(sesid, tableid, JET_MoveFirst, 0));
	Call(JetRetrieveColumn(sesid, tableid, columnid, buffer, sizeof(buffer), &retrievedSize, 0, NULL));
	buffer[retrievedSize] = 0;
	printf("%s", buffer);

    /* Terminate ESENT */
    JetCloseTable(sesid, tableid);
    JetEndSession(sesid, 0);
    JetTerm(instance);
	return 0;

HandleError:
	printf("ESENT error %d\n", err);
	return 1; 
}
