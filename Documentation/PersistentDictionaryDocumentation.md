# Data Storage with PersistentDictionary
PersistentDictionary is a database-backed dictionary built on top of the ESENT database engine. It is a drop-in compatible replacement for the generic _Dictionary_<TKey,TValue>, _SortedDictionary_<TKey, TValue> and _SortedList_<TKey, TValue> classes found in the System.Collections.Generic namespace.

```c#
class PersistentDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDisposable
    where TKey : IComparable<TKey>
```

## PersistentDictionary Features
* **No setup**: the ESENT database engine is part of Windows and requires no setup. EsentCollections will work on any version of Windows from XP onwards.
* **Performance**: ESENT supports a high rate of updates and retrieves. Write-ahead logging decreases the cost of making small updates to the data. Information is inserted into or retrieved from the database in-process so data access has very low overhead. B-trees give O(log n) access to data by key and the records are stored in sorted order.
* **Simplicity**: a PersistentDictionary looks and behaves like the .NET Dictionary/SortedDictionary/SortedList classes. No extra method calls are required.
* **Administration-free**: ESENT automatically manages the database cache size, transaction logfiles, and crash recovery so no database administration is needed. The code is structured so that there are no deadlocks or conflicts, even when multiple threads use the same dictionary. ESENT runs in-process and doesn't expose any network access, providing a high degree of security.
* **Reliability**: ESENT's write-ahead logging system means that a database is automatically recovered after a process crash or unexpected machine shutdown (e.g. power outage). Database transactions are used to ensure the logical consistency of the database.
* **Concurrency**: each data structure can be accessed by multiple threads. Reads are non-blocking and updates to different items in the collection are allowed to proceed concurrently.
* **Scale**: A collection can contain up to 2^31 objects, and values can be up to 2GB in size. The maximum database size is 16TB . 

## Sample Code
Here is an application that remembers a first name â†’ last name mapping in a persistent dictionary.
```c#
using System;
using Microsoft.Isam.Esent.Collections.Generic;

namespace PersistentDictionarySample
{
    public static class HelloWorld
    {
        public static void Main()
        {
            var dictionary = new PersistentDictionary<string, string>("Names");
            Console.WriteLine("What is your first name?");
            string firstName = Console.ReadLine();
            if (dictionary.ContainsKey(firstName))
            {
                Console.WriteLine("Welcome back {0} {1}",
                    firstName,
                    dictionary[firstName](firstName));
            }
            else
            {
                Console.WriteLine(
                    "I don't know you, {0}. What is your last name?",
                    firstName);
                dictionary[firstName](firstName) = Console.ReadLine();
            }
        }
    }
}
```

### Sample Code in other languages
[VB.NET Sample](VbNetDictionarySample.md)

[C++/CLI Sample](CppDictionarySample.md)

## Sample Application
A more sophisticated [sample application](SystemStats.md) that tracks some perf counters and performs a simple LINQ query.

## Quick Start
To get started with PersistentDictionary you should:
1. Download the latest version of the PersistentDictionary project. [latest release](https://github.com/Microsoft/ManagedEsent/releases/latest).
2. Copy Esent.Interop.dll and Esent.Collections.dll to your project.
3. Add a reference to Esent.Collections.dll.
4. Use the namespace 'Microsoft.Isam.Esent.Collections.Generic'.
5. Pick a directory to contain the persistent dictionary. The directory will contain all the files (database, logfiles, and checkpoint) needed for the database that stores the data.
6. Create a PersistentDictionary, passing the directory name into the constructor.
7. Use the PersistentDictionary like an ordinary Dictionary or SortedDictionary.
8. Dispose of the PersistentDictionary when finished with it. If the dictionary isn't disposed then ESENT will have the database open until the instance is finalized.

## Supported Key Types
Only these types are supported as dictionary keys:
` Boolean ` ` Byte ` ` Int16 `
` UInt16 ` ` Int32 ` ` UInt32 `
` Int64 ` ` UInt64 ` ` Float `
` Double ` ` Guid ` ` DateTime `
` TimeSpan ` ` String ` 

## Supported Value Types
Dictionary values can be any of the key types, Nullable<T> versions of the key types, Uri, IPAddress or a serializable structure. A structure is only considered serializable if it meets all these criteria:
* The structure is marked as serializable
* Every member of the struct is either:
	* A primitive data type (e.g. Int32)
	* A String, Uri or IPAddress
	* A serializable structure.
Or, to put it another way, a serializable structure cannot contain any references to a class object. This is done to preserve API consistency. Adding an object to a PersistentDictionary creates a copy of the object though serialization. Modifying the original object will not modify the copy, which would lead to confusing behavior. To avoid those problems the PersistentDictionary will only accept value types as values.

**Can Be Serialized**
```c#
[Serializable]
struct Good
{
    public DateTime? Received;
    public string Name;
    public Decimal Price;
    public Uri Url;
}	
```

**Can't Be Serialized**
```c#
[Serializable]
struct Bad
{
    public byte[] Data; // arrays aren't supported
    public Exception Error; // reference object 
}
```

## PersistentDictionary API
PersistentDictionary implements the generic IDictionary interface. See the MSDN entry for [System.Collections.Generic.IDictionary](http://msdn.microsoft.com/en-us/library/s4ys34ea.aspx) for documentation. Additional methods are:

#### Constructors
_The constructor has to specify the location of the database files._
 - `PersistentDictionary(string directory)` : Create a PersistentDictionary in the specified directory.
 - `PersistentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> dictionary, string directory)` : Create a PersistentDictionary in the specified directory, copying entries from the specified collection.

#### Methods
 - `PersistentDictionary.Flush()` :  Force all changes made to this dictionary to be written to disk.

#### File Manipulation
_These are static methods of the PersistentDictionaryFile class._
 - `PersistentDictionaryFile.Exists(string directory)` : Determine if a dictionary database file exists in the specified directory.
 - `PersistentDictionaryFile.DeleteFiles(string directory)` : Delete all files associated with a PersistedDictionary database from the specified directory.

## LINQ Support
A PersistentDictionary can optimize some LINQ operators by retrieving only matching records from the database. For this to happen the LINQ query should specify a subset of item keys using comparison operators. Supported operators are: <, <=, ==, !=, >, >=, Equals, CompareTo and StartsWith (for strings). LINQ queries that only examine values cannot be optimized at all.

Besides 'where', these LINQ operators can be optimized: Any, Min, Max, First, FirstOrDefault, Last, LastOrDefault, Single, SingleOrDefault, Count, Reverse.

Examples of LINQ statements which have efficient support:

```c#
var q = persistentDictionary.Where(x => x.Key < 5 && x.Key > 2 && x.Key != 4).Reverse();
var q = from x in persistentDictionary where x.Key.StartsWith("de") || x.Key.StartsWith("bi") select x.Value;
if (persistentDictionary.Keys.Any(x => x < 5)) { ... }
var q = from x in persistentDictionary where (x.Key.CompareTo("a") > 0 && x.Key.CompareTo("c") <= 0) select x.Value;
```

## Data Consistency
Each database update is performed in a separate transaction and the logfile and database updates are performed in the background. This means that dictionary updates are atomic and consistent, but not always durable. If the application crashes only updates whose log records have been written to disk will be recovered. It is possible to force the dictionary updates created so far to be persisted to disk using **PersistentDictionary.Flush()**. Note that every Flush() call requires a disk I/O so using this too frequently will severely limit the update rate. Disposing the dictionary also flushes all its changes to disk.

## Performance Measurements
These are very basic performance measurements made with a PersistentDictionary<long, string> where the string data was 64 bytes in length. These measurements were taken on my desktop system.

| Operation          | Performance                               |
| ------------------ |:-----------------------------------------:|
| Sequential inserts | 32,000 entries/second                     |
| Random inserts | 17,000 entries/second |
| Random Updates | 36,000 entries/second |
| Random lookups (database cached in memory) | 137,000 entries/second |
| Linq queries (range of records) | 14,000 queries/second |

Performance will vary from application to application. Factors that affect performance include:
* Total number of items in the dictionary.
* Size of the data. Retrieving an int will be faster than retrieving a 55MB string.
* How much of the database is cached in memory. When first opening a populated dictionary the data will not be cached in memory so initial lookups will be slow.
* Update patterns. Sequential inserts are much faster than random inserts.
* Structure serialization. Using structures as dictionary data can be much slower than using basic data types.
* Disk performance.

## File Management
The PersistentDictionary code will automatically open or create a database in the specified directory. To determine if a dictionary database already exists use **PersistentDictionaryFile.Exists()**. To remove a dictionary use **PersistentDictionaryFile.DeleteFiles()**. 
