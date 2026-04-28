# AccountBook Design Brief

## Overview
This document defines a unified design direction for both the desktop application and a matching web dashboard version of AccountBook. The visual language should feel focused, polished, and finance-oriented: dark surfaces, calm teal structure, warm amber highlights, and dense but readable information cards.

The current desktop UI already expresses the core tone. The web version should preserve that identity while adapting the information architecture for responsive layouts and browser navigation patterns.

## Design Goals
- Keep the atmosphere calm, premium, and trustworthy.
- Preserve fast transaction entry as the primary job to be done.
- Make analysis and account management feel like natural extensions, not separate products.
- Use a single shared token system across app and web.
- Ensure the web layout works well on desktop, tablet, and mobile widths.

## Shared Visual System

### Color Tokens
- App Background: `#0A0F14`
- Paper Background: `#111820`
- Panel Background: `#151F28`
- Raised Panel: `#1A2631`
- Sidebar Background: `#0D141B`
- Header Primary: `#1587A8`
- Header Secondary: `#0E6C88`
- Text Primary: `#EEF5F7`
- Text Secondary: `#8FA7B5`
- Text Muted: `#6B808D`
- Accent Primary: `#F0A83A`
- Accent Strong: `#FFC96B`
- Line / Stroke: `#24333E`
- Hover Surface: `#1A2632`
- Positive Chart: `#52D0B2`
- Expense Chart: `#F4A261`
- Warning Surface: `#3B2021`
- Warning Stroke: `#70403C`
- Warning Text: `#F6C1B3`

### Typography
- Primary font: `Malgun Gothic`
- Display title: 34 / Bold
- Section title: 23 / SemiBold
- Header title: 22 / Bold
- Metric value: 28 / SemiBold
- Body: 13 / Regular with 21 line height
- Eyebrow / labels: 12 to 13 / SemiBold

### Radius and Spacing
- Outer shell radius: 24 to 28
- Panel radius: 22
- Input radius: 14 to 18
- Tight gap: 8
- Base gap: 12
- Content gap: 18
- Section gap: 24
- Page margin: 24

### Visual Style Notes
- Avoid flat empty areas. Prefer framed groups and layered cards.
- Keep shadows subtle or omit them. Separation should mostly come from tone shifts and strokes.
- Buttons should feel solid and slightly rounded, not overly glossy.
- Charts should sit inside framed dark wells rather than on open background.

## Desktop App Frame

### Window
- Size target: 1520 x 980
- Minimum size: 1380 x 900
- Two-row structure: fixed top header and flexible content area

### Top Header
- Height: 78
- Full width teal bar
- Left: app mark, product title, current month
- Center: search-like summary pill
- Right: ledger count chip and CSV export button

### Main Body
- Horizontal split
- Left sidebar width: 246
- Gap between sidebar and content: 22
- Right content fills remaining width

### Sidebar
Sections:
- Intro tile with short product explanation
- Navigation buttons: Transaction Entry, Analysis, Settings
- Quick guide tile at the bottom

Interaction:
- Selected nav uses hover surface fill
- Default nav stays transparent
- All tiles use dark card surfaces with light strokes

### Main Card
Top area:
- Eyebrow: `DAILY LEDGER`
- Large title: transaction dashboard headline
- Short supporting paragraph
- Month chip at upper right

Content states:
- Entry state
- Analysis state
- Settings state

## Desktop Entry Screen

### Left Panel: Transaction Form
Fields:
- Date
- Description
- From account
- To account
- Amount
- Memo
- Primary action button
- Validation message area

Layout:
- Two-column account selector row
- Scrollable form card
- Comfortable vertical rhythm, optimized for repeated input

### Right Panel: Preview and Summary
Top:
- Entry preview card showing selected flow and transaction summary

Middle:
- Account summary table in dark mode

Bottom:
- Tips card
- Recommended flow card

## Desktop Analysis Screen

### KPI Strip
- Four metric cards in one row
- Net asset, total expense, account count, entry count

### Filter Panel
- Start date
- End date
- Refresh button
- Summary period copy

### Chart Area
- Left: net asset line chart
- Right: expense line chart
- Each chart inside its own framed card
- Labels aligned below line area

## Desktop Settings Screen

### Purpose
Manage account categories as modular grouped cards.

### Layout
- Three-column responsive grid inside the main content card
- Asset, Liability, Expense, Equity, Revenue cards
- Each card contains account rows and add button

### Account Popup
- Centered modal
- Dimmed overlay
- Type badge
- Name and description inputs
- Validation message
- Primary save and secondary cancel actions

## Web Product Direction
The web version should not be a literal port of the app window. It should feel native to the browser while clearly belonging to the same product family.

### Product Structure
Use a responsive web shell with:
- Sticky top navigation
- Optional collapsible left sidebar on desktop
- Main dashboard canvas with cards and charts
- Faster access to recent activity and summaries

### Web Information Architecture
Top navigation:
- Brand
- Global search or quick jump
- Date range control
- Add transaction CTA
- Theme toggle
- Profile / settings trigger

Primary desktop layout:
- Left sidebar for sections
- Center main dashboard content
- Right assist rail for recent activity, alerts, or quick insights

Tablet layout:
- Collapse assist rail below main content
- Sidebar becomes icon rail or drawer

Mobile layout:
- Single column
- Sticky bottom CTA or top action bar for new transaction
- KPI cards become horizontal scroll or 2-column grid
- Charts stack vertically

## Web Home Dashboard

### Hero Row
- Title: monthly ledger overview
- Supporting text: short summary of current financial state
- Primary CTA: add transaction
- Secondary CTA: export or open analysis

### KPI Section
- 4 cards minimum
- Same metrics as desktop analysis
- Add mini delta or sparkline where useful

### Main Content Grid
Left large column:
- Transaction composer card
- Net asset chart
- Expense chart

Right column:
- Recent transactions list
- Account balance summary
- Smart tips / warnings

## Web Transaction Entry Page

### Focus
Fast browser-first entry for repeated daily use.

### Layout
Desktop:
- Two-column form and live preview
- Sticky action footer within form card

Mobile:
- Single-column stacked inputs
- Large segmented account selectors
- Full-width primary button

### Components
- Date picker
- Description input
- Two account selectors
- Amount input with clear currency emphasis
- Memo area
- Validation feedback block
- Live journal preview card

## Web Analysis Page

### Content Blocks
- Date filters with presets
- KPI cards
- Net asset trend chart
- Expense trend chart
- Account type breakdown
- Recent anomalies or warnings panel

### Chart Guidance
- Keep charts wide and calm
- Use subtle horizontal grid lines only
- Put totals in top-right of each chart card

## Web Settings / Chart of Accounts

### Layout
Desktop:
- Masonry-like or equal-height multi-card grid
- Each account category remains a dedicated card

Mobile:
- Accordion sections by account type

### Row Pattern
Each account row shows:
- Name
- Short description
- Code
- Optional action menu

## Figma Page Recommendation
Create the following pages or sections in one Figma file:
- `00 Foundations`
- `01 Desktop App`
- `02 Web Dashboard`
- `03 Web Entry`
- `04 Web Analysis`
- `05 Web Settings`
- `06 Components`

## Figma Frames To Create
- Desktop / Entry / 1520
- Desktop / Analysis / 1520
- Desktop / Settings / 1520
- Desktop / Modal / Add Account
- Web / Dashboard / 1440
- Web / Entry / 1440
- Web / Analysis / 1440
- Web / Settings / 1440
- Web / Dashboard / 1024
- Web / Dashboard / 390
- Web / Entry / 390

## Component List
- App header
- Sidebar navigation item
- KPI card
- Primary button
- Secondary button
- Text input
- Text area
- Combo/select field
- Date field
- Month chip
- Warning message block
- Account row
- Recent transaction row
- Chart card
- Modal shell

## Motion Notes
- Use gentle fade and rise for card entrance
- Use quick surface tint transitions on hover
- Avoid flashy finance-dashboard animation

## Implementation Notes
- Keep the dark theme as the default shared identity.
- Light theme can stay as an alternate mode, but Figma should prioritize dark mode first.
- Web should preserve the same token names when implemented in code.
- Do not over-compress the layout; this product benefits from breathing room around grouped information.
