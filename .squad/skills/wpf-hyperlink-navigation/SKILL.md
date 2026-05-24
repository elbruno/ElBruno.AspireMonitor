---
name: "wpf-hyperlink-navigation"
description: "Open browser URLs from WPF views using Hyperlink and RequestNavigate"
domain: "wpf-ui"
confidence: "medium"
source: "earned"
---

## Context

Use this when a WPF screen needs clickable URLs that open in the user's default browser.

## Patterns

- Prefer `<Hyperlink NavigateUri="...">` inside a `TextBlock` over mouse click handlers for links.
- Handle `RequestNavigate` in code-behind and call a centralized `OpenUrl(string)` helper.
- Keep `TextDecorations="Underline"` and a link color for discoverability.

## Examples

```xml
<TextBlock>
  <Hyperlink NavigateUri="{Binding HostUrl}"
             RequestNavigate="Url_RequestNavigate">
    <Run Text="{Binding HostUrl}"/>
  </Hyperlink>
</TextBlock>
```

```csharp
private void Url_RequestNavigate(object sender, RequestNavigateEventArgs e)
{
    OpenUrl(e.Uri.AbsoluteUri);
    e.Handled = true;
}
```

## Anti-Patterns

- Using `MouseLeftButtonDown` for links when `Hyperlink` works.
- Duplicating `Process.Start` logic in multiple views.
- Binding non-Visibility properties through visibility-only converters.
