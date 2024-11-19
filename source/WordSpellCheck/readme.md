### Important Notes:
- **COM Interop and Garbage Collection:** When using COM objects, it's important to properly release them to avoid memory leaks. Use `Marshal.ReleaseComObject` and manually set objects to `null` followed by `GC.Collect()` and `GC.WaitForPendingFinalizers()` calls.
- **Interop Dependencies:** Ensure that you have the corresponding version of Microsoft Office installed as the Interop libraries rely on the Office installation.
- **Permissions and File Paths:** The code should have the necessary permissions to write to the specified file path.