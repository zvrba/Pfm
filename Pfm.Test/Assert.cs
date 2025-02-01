using System;

namespace Podaga.PersistentCollections.Test;

internal static class Assert
{
    const string Message = "Assertion failed due to invalid implementation.";

    public static void True(bool condition) {
        if (!condition)
            throw new NotImplementedException(Message);
    }

    public static void Throws<E>(Action a) where E : Exception {
        try { a(); }
        catch (E) { return; }
        catch { }
        throw new NotImplementedException(Message);
    }
}
