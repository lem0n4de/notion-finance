using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;

namespace NotionFinance.Tests;

public class PropertiesPostprocessor : ISpecimenBuilder
{
    private readonly ISpecimenBuilder builder;

    public PropertiesPostprocessor(ISpecimenBuilder builder)
    {
        this.builder = builder;
    }

    public object Create(object request, ISpecimenContext context)
    {
        dynamic s = this.builder.Create(request, context);
        if (s is NoSpecimen)
            return s;

        s.SetupAllProperties();
        return s;
    }
}

public class AutoMoqPropertiesCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customizations.Add(
            new PropertiesPostprocessor(
                new MockPostprocessor(
                    new MethodInvoker(
                        new MockConstructorQuery()))));
        fixture.ResidueCollectors.Add(new MockRelay());
    }
}

public class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute() : base(new Fixture()
        .Customize(new AutoMoqCustomization() {ConfigureMembers = true}))
    {
    }
}