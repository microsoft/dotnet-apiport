## 134: Persian calendar now uses the Hijri solar algorithm

### Scope
Minor

### Version Introduced
4.6

### Source Analyzer Status
Available

### Change Description
Starting with the .NET Framework 4.6, the PersianCalendar class uses the Hijri solar algorithm. Converting dates between the PersianCalendar and other calendars may produce a slightly different result beginning with the .NET Framework 4.6 for dates earlier than 1800 or later than 2023 (Gregorian).

Also, `PersianCalendar.MinSupportedDateTime` is now `March 22, 0622 instead of March 21, 0622`.

- [ ] Quirked
- [ ] Build-time break

### Recommended Action
Be aware that some early or late dates may be slightly different when using the PersianCalendar in .NET 4.6. Also, when serializing dates between processes which may run on different .NET Framework versions, do not store them as PersianCalendar date strings (since those values may be different).

### Affected APIs
* `F:System.Globalization.PersianCalendar.PersianEra`
* `M:System.Globalization.PersianCalendar.#ctor`
* `M:System.Globalization.PersianCalendar.AddMonths(System.DateTime,System.Int32)`
* `M:System.Globalization.PersianCalendar.AddYears(System.DateTime,System.Int32)`
* `M:System.Globalization.PersianCalendar.get_AlgorithmType`
* `M:System.Globalization.PersianCalendar.get_Eras`
* `M:System.Globalization.PersianCalendar.get_MaxSupportedDateTime`
* `M:System.Globalization.PersianCalendar.get_MinSupportedDateTime`
* `M:System.Globalization.PersianCalendar.get_TwoDigitYearMax`
* `M:System.Globalization.PersianCalendar.GetDayOfMonth(System.DateTime)`
* `M:System.Globalization.PersianCalendar.GetDayOfWeek(System.DateTime)`
* `M:System.Globalization.PersianCalendar.GetDayOfYear(System.DateTime)`
* `M:System.Globalization.PersianCalendar.GetDaysInMonth(System.Int32,System.Int32,System.Int32)`
* `M:System.Globalization.PersianCalendar.GetDaysInYear(System.Int32,System.Int32)`
* `M:System.Globalization.PersianCalendar.GetEra(System.DateTime)`
* `M:System.Globalization.PersianCalendar.GetLeapMonth(System.Int32,System.Int32)`
* `M:System.Globalization.PersianCalendar.GetMonth(System.DateTime)`
* `M:System.Globalization.PersianCalendar.GetMonthsInYear(System.Int32,System.Int32)`
* `M:System.Globalization.PersianCalendar.GetYear(System.DateTime)`
* `M:System.Globalization.PersianCalendar.IsLeapDay(System.Int32,System.Int32,System.Int32,System.Int32)`
* `M:System.Globalization.PersianCalendar.IsLeapMonth(System.Int32,System.Int32,System.Int32)`
* `M:System.Globalization.PersianCalendar.IsLeapYear(System.Int32,System.Int32)`
* `M:System.Globalization.PersianCalendar.set_TwoDigitYearMax(System.Int32)`
* `M:System.Globalization.PersianCalendar.ToDateTime(System.Int32,System.Int32,System.Int32,System.Int32,System.Int32,System.Int32,System.Int32,System.Int32)`
* `M:System.Globalization.PersianCalendar.ToFourDigitYear(System.Int32)`
* `P:System.Globalization.PersianCalendar.AlgorithmType`
* `P:System.Globalization.PersianCalendar.Eras`
* `P:System.Globalization.PersianCalendar.MaxSupportedDateTime`
* `P:System.Globalization.PersianCalendar.MinSupportedDateTime`
* `P:System.Globalization.PersianCalendar.TwoDigitYearMax`
* `T:System.Globalization.PersianCalendar`

### Category
Core

[More information](https://msdn.microsoft.com/en-us/library/dn833125%28v=vs.110%29.aspx#Core)
