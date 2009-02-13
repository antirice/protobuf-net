﻿using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using NUnit.Framework;
using ProtoBuf;
using System.Runtime.Serialization;

namespace Examples
{
    [ProtoContract]
    public class DetectMissing
    {
        private int? foo;
        
        [ProtoMember(1)]
        public int Foo
        {
            get { return foo ?? 5; }
            set { foo = value;}
        }
        [XmlIgnore, Browsable(false)]
        public bool FooSpecified
        {
            get { return foo != null; }
            set { foo = value ? Foo : (int?)null;}
        }

        private bool ShouldSerializeFoo() {return FooSpecified; }
        private void ResetFoo() { FooSpecified = false; }

        private string bar;
        
        [ProtoMember(2)]
        public string Bar
        {
            get { return bar ?? "abc"; }
            set { bar = value;}
        }
        [XmlIgnore, Browsable(false)]
        public bool BarSpecified
        {
            get { return bar != null; }
            set { bar = value ? Bar : null;}
        }
        private bool ShouldSerializeBar() { return BarSpecified; }
        private void ResetBar() { BarSpecified = false; }

    }

    [TestFixture]
    public class TestDetectMissing
    {
        [Test]
        public void TestDefaults()
        {
            DetectMissing dm = new DetectMissing();
            Assert.AreEqual(5, dm.Foo);
            Assert.AreEqual("abc", dm.Bar);
            Assert.IsFalse(dm.FooSpecified, "FooSpecified");
            Assert.IsFalse(dm.BarSpecified, "BarSpecified");
        }

        [Test]
        public void TestSetValuesToDefaults()
        {
            DetectMissing dm = new DetectMissing();
            dm.Foo = 5;
            dm.Bar = "abc";
            Assert.AreEqual(5, dm.Foo);
            Assert.AreEqual("abc", dm.Bar);
            Assert.IsTrue(dm.FooSpecified, "FooSpecified");
            Assert.IsTrue(dm.BarSpecified, "BarSpecified");
        }

        [Test]
        public void TestSetValuesToNewValues()
        {
            DetectMissing dm = new DetectMissing();
            dm.Foo = 7;
            dm.Bar = "def";
            Assert.AreEqual(7, dm.Foo);
            Assert.AreEqual("def", dm.Bar);
            Assert.IsTrue(dm.FooSpecified, "FooSpecified");
            Assert.IsTrue(dm.BarSpecified, "BarSpecified");
        }

        [Test]
        public void TestSetSpecified()
        {
            DetectMissing dm = new DetectMissing();
            dm.FooSpecified = true;
            dm.BarSpecified = true;
            Assert.AreEqual(5, dm.Foo);
            Assert.AreEqual("abc", dm.Bar);
            Assert.IsTrue(dm.FooSpecified, "FooSpecified");
            Assert.IsTrue(dm.BarSpecified, "BarSpecified");
        }

        [Test]
        public void TestResetSpecified()
        {
            DetectMissing dm = new DetectMissing();
            dm.Foo = 27;
            dm.Bar = "ghi";
            dm.FooSpecified = false;
            dm.BarSpecified = false;
            Assert.AreEqual(5, dm.Foo);
            Assert.AreEqual("abc", dm.Bar);
            Assert.IsFalse(dm.FooSpecified, "FooSpecified");
            Assert.IsFalse(dm.BarSpecified, "BarSpecified");
        }

        [Test]
        public void TestViaXmlSerializerNotSet()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer ser = new XmlSerializer(typeof(DetectMissing));
                ser.Serialize(ms, new DetectMissing());
                ms.Position = 0;
                DetectMissing dm = (DetectMissing) ser.Deserialize(ms);
                Assert.IsFalse(dm.FooSpecified, "FooSpecified");
                Assert.IsFalse(dm.BarSpecified, "BarSpecified");
            }
        }
        [Test]
        public void TestViaXmlSerializerSet() {
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer ser = new XmlSerializer(typeof(DetectMissing));
                ser.Serialize(ms, new DetectMissing { FooSpecified = true, BarSpecified = true});
                ms.Position = 0;
                DetectMissing dm = (DetectMissing)ser.Deserialize(ms);
                Assert.IsTrue(dm.FooSpecified, "FooSpecified");
                Assert.IsTrue(dm.BarSpecified, "BarSpecified");
            }
        }

        [Test]
        public void TestViaXmlProtoNotSet()
        {
            DetectMissing dm = Serializer.DeepClone(new DetectMissing());
            Assert.IsFalse(dm.FooSpecified, "FooSpecified");
            Assert.IsFalse(dm.BarSpecified, "BarSpecified");
        }
        [Test]
        public void TestViaXmlProtoSet()
        {
            DetectMissing dm = Serializer.DeepClone(new DetectMissing
                {FooSpecified = true, BarSpecified = true});
            Assert.IsTrue(dm.FooSpecified, "FooSpecified");
            Assert.IsTrue(dm.BarSpecified, "BarSpecified");
        }

        [Test]
        public void TestComponentModelNotSet()
        {
            DetectMissing dm = new DetectMissing();
            var props = TypeDescriptor.GetProperties(dm);
            Assert.IsFalse(props["Foo"].ShouldSerializeValue(dm), "Foo");
            Assert.IsFalse(props["Bar"].ShouldSerializeValue(dm), "Bar");
        }

        [Test]
        public void TestComponentModelSet()
        {
            DetectMissing dm = new DetectMissing {FooSpecified = true, BarSpecified = true};
            var props = TypeDescriptor.GetProperties(dm);
            Assert.IsTrue(props["Foo"].ShouldSerializeValue(dm), "Foo");
            Assert.IsTrue(props["Bar"].ShouldSerializeValue(dm), "Bar");
        }

        [Test]
        public void TestComponentModelReset()
        {
            DetectMissing dm = new DetectMissing { Foo = 37, Bar = "fgjh" };
            var props = TypeDescriptor.GetProperties(dm);
            Assert.IsTrue(props["Foo"].CanResetValue(dm), "Foo");
            Assert.IsTrue(props["Bar"].CanResetValue(dm), "Bar");
            props["Foo"].ResetValue(dm);
            props["Bar"].ResetValue(dm);
            Assert.IsFalse(dm.FooSpecified, "Foo");
            Assert.IsFalse(dm.BarSpecified, "Bar");
        }
    }
}
