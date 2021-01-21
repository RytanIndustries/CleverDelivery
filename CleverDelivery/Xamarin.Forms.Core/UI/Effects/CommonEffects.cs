using Xamarin.Forms;

[assembly: ResolutionGroupName("CoreEffects")]
namespace Xamarin.Forms.Core
{
#if !WINDOWS_UWP
    public class RemoveEmptyRowsEffect : RoutingEffect
    {
        public RemoveEmptyRowsEffect() : base($"CoreEffects.{typeof(ListRemoveEmptyRows).Name}") { }
    }
#endif
#if __IOS__

	public class DisableWebViewScrollEffect : RoutingEffect
	{
		public DisableWebViewScrollEffect() : base($"CoreEffects.{typeof(WKWebViewDisableScroll).Name}") { }
	}

#endif
#if !WINDOWS_UWP
    public class HideListSeparatorEffect : RoutingEffect
	{
		public HideListSeparatorEffect() : base($"CoreEffects.{typeof(HideTableSeparator).Name}") { }
	}
    public class UnderlineColorEffect : RoutingEffect
    {
        public UnderlineColorEffect() : base($"CoreEffects.{typeof(UnderlineColor).Name}") { }
    }
#endif

}
