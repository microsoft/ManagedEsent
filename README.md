# ESENT Managed Interop
ManagedEsent provides managed access to esent.dll, the embeddable database engine native to Windows.

The **[Microsoft.Isam.Esent.Interop](Documentation/ManagedEsentDocumentation.md)** namespace in EsentInterop.dll provides managed access to the basic ESENT API. Use this for applications that want access to the full ESENT feature set.

The **[PersistentDictionary](Documentation/PersistentDictionaryDocumentation.md)** class in EsentCollections.dll provides a persistent, generic dictionary for .NET, with LINQ support. A PersistentDictionary is backed by an ESENT database and can be used to replace a standard Dictionary, HashTable, or SortedList. Use it when you want extremely simple, reliable and fast data persistence.

**esedb** provides both dbm and shelve modules built on top of ESENT IronPython users.