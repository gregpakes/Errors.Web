# Errors.Web

Demonstrates a URL Rewrite issue.

- Set a breakpoint in the Context_Error event in the ErrorHandlerModule HttpModule
- Navigate to /fake/'select * from tbl;
- Watch the breakpoint get hit twice.
