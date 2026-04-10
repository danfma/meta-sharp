using Metano.Annotations;

namespace SampleTodo.Service.Js.Hono;

[Import(name: "Hono", from: "hono", Version = "^4.6.0")]
public class Hono
{
    public Hono() { }

    // Sync handler form: `(c) => c.Json(...)`. Used for routes that don't need to
    // read the request body.
    [Name("get")]    public void Get(string path, Func<IHonoContext, IHonoContext> handler) => throw new NotSupportedException();
    [Name("delete")] public void Delete(string path, Func<IHonoContext, IHonoContext> handler) => throw new NotSupportedException();

    // Async handler form: `async (c) => { var body = await c.Req.Json<T>(); ... }`.
    // Required for routes that consume the request body, since `req.json()` is async.
    [Name("post")]  public void Post(string path, Func<IHonoContext, Task<IHonoContext>> handler) => throw new NotSupportedException();
    [Name("patch")] public void Patch(string path, Func<IHonoContext, Task<IHonoContext>> handler) => throw new NotSupportedException();
    [Name("put")]   public void Put(string path, Func<IHonoContext, Task<IHonoContext>> handler) => throw new NotSupportedException();
}
