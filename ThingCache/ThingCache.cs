using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace MockFramework
{
    public class ThingCache
    {
        private readonly IDictionary<string, Thing> dictionary
            = new Dictionary<string, Thing>();
        private readonly IThingService thingService;

        public ThingCache(IThingService thingService) => this.thingService = thingService;

        public Thing Get(string thingId)
        {
            if (dictionary.TryGetValue(thingId, out var thing))
                return thing;
            if (!thingService.TryRead(thingId, out thing)) return null;
            dictionary[thingId] = thing;
            return thing;
        }
    }

    [TestFixture]
    public class ThingCache_Should
    {
        private IThingService thingService;
        private ThingCache thingCache;

        private const string thingId1 = "TheDress";
        private Thing thing1 = new Thing(thingId1);

        private const string thingId2 = "CoolBoots";
        private Thing thing2 = new Thing(thingId2);

        [SetUp]
        public void SetUp()
        {
            thingService = A.Fake<IThingService>();
            thingCache = new ThingCache(thingService);
        }

        [Test]
        public void Get_ReturnsPutThing()
        {
            var thing = thing1;
            A.CallTo(() => thingService.TryRead(thingId1, out thing))
                .Returns(true);

            thingCache.Get(thingId1).Should().Be(thing1);
        }

        [Test]
        public void Get_ReturnsThingFromCache()
        {
            var thing = thing1;
            A.CallTo(() => thingService.TryRead(thingId1, out thing1)).Returns(true);
            
            thingCache.Get(thingId1);
            thingCache.Get(thingId1);
            A.CallTo(() => thingService.TryRead(thingId1, out thing))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void Get_ReturnsNull_WhenUnknownThingId()
        {
            Thing _ = null;
            A.CallTo(() => thingService.TryRead(A<string>.Ignored, out _))
                .Returns(false);

            thingCache.Get(string.Empty).Should().BeNull();
        }

        [Test]
        public void Get_ReturnsCorrectValuesFromCache_WhenThereAreSeveralThings()
        {
            A.CallTo(() => thingService.TryRead(thingId1, out thing1)).Returns(true);
            A.CallTo(() => thingService.TryRead(thingId2, out thing2)).Returns(true);

            thingCache.Get(thingId1);
            thingCache.Get(thingId1);
            thingCache.Get(thingId2);
            thingCache.Get(thingId2);
            
            A.CallTo(() => thingService.TryRead(A<string>.Ignored, out thing1))
                .MustHaveHappened(Repeated.Exactly.Twice);
        }
    }
}