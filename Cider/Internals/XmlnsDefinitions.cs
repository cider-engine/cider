using Cider.Attributes;

[assembly: XmlnsDefinition(XmlnsStatics.Engine, "Cider.Scenes")]
[assembly: XmlnsDefinition(XmlnsStatics.Engine, "Cider.Components")]
[assembly: XmlnsDefinition(XmlnsStatics.Engine, "Cider.Components.In2D")]
[assembly: XmlnsDefinition(XmlnsStatics.Engine, "Cider.Components.In2D.Controls")]
[assembly: XmlnsDefinition(XmlnsStatics.Engine, "Cider.Components.In2D.Physics")]


[assembly: XmlnsDefinition(XmlnsStatics.Engine, "Cider.Data.In2D")]


[assembly: XmlnsDefinition(XmlnsStatics.Engine, "Cider.Project")]


[assembly: XmlnsDefinition(XmlnsStatics.Engine, "Cider.Physics")]


[assembly: XmlnsDirectType(XmlnsStatics.Engine, typeof(System.Numerics.Vector2))]

static class XmlnsStatics
{
    public const string Engine = "https://github.com/cider-engine";
}