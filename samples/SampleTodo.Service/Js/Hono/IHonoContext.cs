using MetaSharp.Annotations;

namespace SampleTodo.Service.Js.Hono;

/// <summary>
/// Structural shape of Hono's <c>Context</c> object. Decorated with <c>[NoEmit]</c>
/// so MetaSharp recognizes the type in C# (parameter signatures, member access)
/// without producing any TypeScript declaration — at runtime the user's transpiled
/// code receives the real Hono context, and TypeScript infers its type from Hono's
/// types via the <c>app.get/post/...</c> handler signature.
/// </summary>
[NoEmit]
public interface IHonoContext
{
    [Name("text")] IHonoContext Text(string text);
    [Name("text")] IHonoContext Text(string text, int status);

    [Name("json")] IHonoContext Json<T>(T value);
    [Name("json")] IHonoContext Json<T>(T value, int status);

    /// <summary>
    /// Returns a response with no body. Use for status codes that disallow content
    /// (204 No Content, 304 Not Modified, etc.) — Hono's <c>text()</c> overload
    /// rejects those because its return type is restricted to ContentfulStatusCode.
    /// </summary>
    [Name("body")] IHonoContext Body(string? data, int status);

    [Name("notFound")] IHonoContext NotFound();

    IHonoRequest Req { [Name("req")] get; }
}

/// <summary>
/// Structural shape of Hono's <c>HonoRequest</c>. Same <c>[NoEmit]</c> story as
/// <see cref="IHonoContext"/> — types only, no emitted TS declaration.
/// </summary>
[NoEmit]
public interface IHonoRequest
{
    /// <summary>
    /// Reads a path parameter (e.g., <c>:id</c>). Returns null when the key isn't
    /// declared in the route pattern.
    /// </summary>
    [Name("param")] string? Param(string key);

    /// <summary>
    /// Parses the request body as JSON and casts to <typeparamref name="T"/>. The
    /// runtime is Hono's plain JSON parser — no validation. Pair with
    /// <c>[PlainObject]</c> DTOs to keep the type contract honest at the boundary.
    /// </summary>
    [Name("json")] Task<T> Json<T>();
}
