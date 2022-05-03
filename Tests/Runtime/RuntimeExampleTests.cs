using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;

public class RuntimeExampleTests
{
    // A Test behaves as an ordinary method
    [Test]
    public void SimplePasses()
    {
        // Use the Assert class to test conditions
        Assert.AreEqual(2, 2);
    }

    [Test]
    public void InputSystemWorksProperly() {
        Assert.AreEqual(InputSystem.version.ToString(), "1.0.2.0");
        Assert.AreNotEqual(InputSystem.version.ToString(), "notaversion");
    }

    [Test]
    public void hasProcessorTest()
    {
        string processorString = "thing(arg1=false)";

        bool trueResult = ProcessorStringHelper.hasProcessor(processorString, "thing");
        bool falseResult = ProcessorStringHelper.hasProcessor(processorString, "owijfoij");

        Assert.IsTrue(trueResult);
        Assert.IsFalse(falseResult);
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator WithEnumeratorPasses()
    {
        Assert.AreEqual(2, 2);

        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
