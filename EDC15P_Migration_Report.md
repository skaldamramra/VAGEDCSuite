# EDC15P Map Rules Migration Report

## Executive Summary

This report documents the progress of migrating map detection rules from `EDC15PFileParser.cs` to `EDC15P_MapRules.xml`. The analysis identifies **migratable** and **non-migratable** map rules based on current XML schema capabilities.

**Latest Update (v2.0):** The XML schema has been extended with new condition elements and metadata features that enable migration of previously non-migratable rules. All major categories are now supported.

---

## Current State (Already Migrated)

The following rules have been successfully migrated to XML:

1. **Launch Control Map** (`launch_control_700`) - Length 700 bytes
2. **Injector Duration 570** (`injector_duration_570`) - Length 570 bytes with multiple axis ID combinations
3. **Boost Target 320** (`boost_target_320`) - Length 320 bytes
4. **Smoke Limiter 416** (`smoke_limiter_416`) - Length 416 bytes

---

## New XML Schema Features (v2.0)

The following features have been added to enable migration of previously non-migratable rules:

### Condition Elements

| Element | Attributes | Description |
|---------|------------|-------------|
| `<ByteCheck>` | `address`, `offset`, `expected`, `op` | Checks raw byte values at specific addresses |
| `<AxisValueCheck>` | `axis`, `comparison`, `value`, `checkType` | Checks actual axis data values at runtime |
| `<MapSelectorProperty>` | `property`, `comparison`, `value` | Checks MapSelector object properties |
| `<CodeBlock>` | `id`, `type` | Checks code block context |

### Metadata Elements

| Element | Attributes | Description |
|---------|------------|-------------|
| `<Name>` | `sequential`, `baseName`, `codeBlockScoped` | Sequential naming with optional code block scoping |
| `<ConditionalTemplate>` | `conditionType`, `condition` | Dynamic naming based on conditions |

---

## Migratable Map Rules (All Categories Now Supported)

### 1. Injector Duration Maps (Various Sizes)

| Length | Axis IDs | Status |
|--------|----------|--------|
| 480 | C5/EC, C4/EA | ✅ Migratable |
| 448 | C5/EC | ✅ Migratable |
| 390 | Any | ✅ Migratable |
| 360 | Any | ✅ Migratable |
| 260 | C5/EC | ✅ Migratable |
| 220 | C5/EC | ✅ Migratable |
| 200 | C4/EA, C4/EC | ✅ Migratable |

### 2. SOI Maps with Temperature Naming

| Length | Axis IDs | Status |
|--------|----------|--------|
| 448 | C5/EC | ✅ Migratable (v2.0) |
| 416 | EA/E9 | ✅ Migratable (v2.0) |
| 308 | Various | ✅ Migratable (v2.0) |

**New Features Used:** `<MapSelectorProperty>` for NumRepeats checks, sequential naming

### 3. Maps with Runtime Axis Value Checks

| Length | Axis IDs | Description | Status |
|--------|----------|-------------|--------|
| 416 | EC/DA | MAP vs MAF limiter | ✅ Migratable (v2.0) |
| 200 | C5/EC | axis value > 3500 check | ✅ Migratable (v2.0) |

**New Features Used:** `<AxisValueCheck>` for max/min value comparisons

### 4. Maps with Byte-Level Checks

| Length | Axis IDs | Description | Status |
|--------|----------|-------------|--------|
| 416 | EC/C0, EC/E9 | Zero-check on Y-axis | ✅ Migratable (v2.0) |
| 416 | EA/E8 | Zero-check on X-axis | ✅ Migratable (v2.0) |

**New Features Used:** `<ByteCheck>` for raw byte inspection

### 5. Maps with Dynamic Sequential Naming

| Length | Axis IDs | Description | Status |
|--------|----------|-------------|--------|
| 416 | F9/DA | Smoke limiter temp variants | ✅ Migratable (v2.0) |
| 384 | EA/DA | Smoke limiter | ✅ Migratable (v2.0) |

**New Features Used:** `<ConditionalTemplate>` for dynamic naming

### 6. Maps with Complex Axis ID Exact Matches

| Length | Axis IDs | Description | Status |
|--------|----------|-------------|--------|
| 200 | C1AE/EC, C178/EC, C1BE/EC | Fuel temp protection | ✅ Migratable (v2.0) |
| 128 | Various | Specific axis combinations | ✅ Migratable (v2.0) |

**New Features Used:** `<XAxisID exact="..." />` with `<Or>` conditions

### 7. Maps Requiring CodeBlock Context

| Map Type | Description | Status |
|----------|-------------|--------|
| Injector Duration | Sequential naming per code block | ✅ Migratable (v2.0) |
| All maps | Code block scoped counting | ✅ Migratable (v2.0) |

**New Features Used:** `codeBlockScoped="true"` on `<Name>`

### 8. Torque Limiter Maps

| Length | Axis Dimensions | Status |
|--------|-----------------|--------|
| 150 | 3x25 | ✅ Migratable |
| 138 | 3x23 | ✅ Migratable |
| 132 | 3x22 | ✅ Migratable |
| 126 | 3x21 | ✅ Migratable |
| 120 | 3x20 | ✅ Migratable |

### 9. Driver Wish Maps

| Length | Axis Dimensions | Status |
|--------|-----------------|--------|
| 286 | 13x11 | ✅ Migratable |
| 256 | Any | ✅ Migratable |
| 240 | 12x10 | ✅ Migratable |
| 216 | 12x9 | ✅ Migratable |

### 10. EGR Maps

| Length | Axis IDs | Status |
|--------|----------|--------|
| 416 | EC/C0 or EC/E9 | ✅ Migratable (v2.0) |
| 390 | EC/C0 | ✅ Migratable |
| 384 | EC/C0 | ✅ Migratable |
| 352 | EC/C0, EC/EA | ✅ Migratable |

### 11. N75 Duty Cycle Maps

| Length | Axis IDs | Status |
|--------|----------|--------|
| 416 | EC/EA, EC/E9D4 | ✅ Migratable (v2.0) |
| 352 | EC/EA | ✅ Migratable |

### 12. MAF Airmass Correction by Temperature

| Length | Description | Status |
|--------|-------------|--------|
| 320 | Default case | ✅ Migratable |

---

## Migration Priority Summary

### Completed
- ✅ All high-priority rules migrated
- ✅ Medium-priority rules with v2.0 schema extensions
- ✅ Complex axis ID combinations
- ✅ Code block context handling

### Remaining Edge Cases
The following still require special handling:
- Maps with special offset calculations (Length 50 with EC axis IDs)
- Maps requiring non-standard axis address layouts

---

## Conclusion

**Current Migration Status (v2.0):**
- ✅ **4 rules** already migrated (original)
- ✅ **~60 additional rules** now migratable with v2.0 schema
- ⚠️ **~5 rules** remain as edge cases (special offset calculations)

**Recommendation:**
1. The XML schema v2.0 now supports migration of all major map rule categories
2. Continue migrating remaining edge case rules as needed
3. Document special offset calculation requirements for future enhancement

---

## Example: New Schema Usage

### ByteCheck Condition
```xml
<ByteCheck address="y_axis_address" offset="0" expected="0x00" op="ne" />
```

### AxisValueCheck Condition
```xml
<AxisValueCheck axis="Y" comparison="lt" value="4000" checkType="max" />
```

### Conditional Template for Dynamic Naming
```xml
<Name sequential="true" baseName="Smoke limiter" codeBlockScoped="true" />
```