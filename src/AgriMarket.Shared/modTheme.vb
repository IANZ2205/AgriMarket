Imports System.Drawing

Public Module modTheme
    ' Palette based on White, Ocean (#016064), Teal (#5FB3B3)
    Public ReadOnly cWhite As Color = Color.FromArgb(255, 255, 255)
    Public ReadOnly cOcean As Color = Color.FromArgb(1, 96, 100)
    Public ReadOnly cTeal As Color = Color.FromArgb(95, 179, 179)

    ' Derived UI Colors
    Public ReadOnly cText As Color = Color.FromArgb(11, 46, 46)
    Public ReadOnly cMuted As Color = Color.FromArgb(95, 124, 124)
    Public ReadOnly cBorder As Color = Color.FromArgb(213, 228, 228)
    Public ReadOnly cSurface As Color = Color.FromArgb(242, 248, 248)

    ' Status Colors
    Public ReadOnly cSuccess As Color = Color.FromArgb(30, 127, 79)
    Public ReadOnly cWarning As Color = Color.FromArgb(178, 106, 0)
    Public ReadOnly cDanger As Color = Color.FromArgb(179, 38, 30)

    ' Typography
    Public ReadOnly fTitle As New Font("Segoe UI", 24, FontStyle.Bold)
    Public ReadOnly fCardTitle As New Font("Segoe UI", 15, FontStyle.Bold)
    Public ReadOnly fBody As New Font("Segoe UI", 10, FontStyle.Regular)
    Public ReadOnly fLabel As New Font("Segoe UI", 9, FontStyle.Regular)
End Module