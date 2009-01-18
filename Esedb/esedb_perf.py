#-----------------------------------------------------------------------
# <copyright file="esedb_perf.py" company="Microsoft Corporation">
# Copyright (c) Microsoft Corporation.
# </copyright>
#-----------------------------------------------------------------------

import esedb
import System
import random

from System.Diagnostics import Stopwatch

database = 'perf.db'

def insertTest(keys):
	db = esedb.open(database, 'n', True)
	data = 'XXXXXXXXXXXXXXXX'
	timer = Stopwatch.StartNew()
	for x in keys:
		db[x] = data
	timer.Stop()
	db.close()
	return timer.Elapsed

def retrieveTest(keys):
	db = esedb.open(database, 'r')
	timer = Stopwatch.StartNew()
	for x in keys:
		data = db[x]
	timer.Stop()
	db.close()
	return timer.Elapsed

def scanTest():
	db = esedb.open(database, 'r')
	timer = Stopwatch.StartNew()
	i = 0
	for (k,v) in db:
		i += 1
	timer.Stop()
	db.close()
	return timer.Elapsed
	
# First insert the records in ascending order, this will be fastest
keys = range(1000000)
time = insertTest(keys)
print 'appended %d records in %s (lazy commit)' % (len(keys), time)

# Now insert in random order (more likely)
random.shuffle(keys)
time = insertTest(keys)
print 'randomly inserted %d records in %s (lazy commit)' % (len(keys), time)

# Now scan all the records in key order. As the database was closed and reopened
# we will be starting with no data cached
time = scanTest()
print 'scanned %d records in %s' % (len(keys), time)

# Now retrieve all the records. As the database was closed and reopened
# we will be starting with no data cached
random.shuffle(keys)
time = retrieveTest(keys)
print 'randomly retrieved %d records in %s' % (len(keys), time)

