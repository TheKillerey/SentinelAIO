   M S B A M L   `   `   `  ����   SentinelAIO)    #SentinelAIO.Themes.SkinEditorWindow   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     c _MaterialDesignThemes.MahApps, Version=3.1.0.0, Culture=neutral, PublicKeyToken=df2a72020bd7962a_ [MaterialDesignThemes.Wpf, Version=5.0.1.0, Culture=neutral, PublicKeyToken=df2a72020bd7962a<md1http://materialdesigninxaml.net/winfx/xaml/themes     � Title$  Skin Editor��$	��500��$	��800�� � WindowStartupLocation$ CenterScreen=�    MaterialDesignDarkBackground#���      MaterialDesignDarkForeground#���  $��14(���:  4MaterialDesignThemes.Wpf.MaterialDesignFontExtension  .��� $��20q� � ColumnDefinitions �� $	��200 ��� $��* �.+�� $��0��$��10q� � RowDefinitions �� $
��Auto ��� $��* �.+��� $��	Champions��$��16(�$��Medium"�$��0,0,0,10q�#���  �� -    ��Name$ ChampionsList��$��1��  MaterialDesignListBox#��[ ����  �� 
TargetType" ��.R��� $
��Auto=�$
��Auto=�.���� � $��1��$��10q� �� $
��Auto ��� $��* ��� $
��Auto �.+��� $��Skins��$��16(�$��Medium"�$��0,0,0,10q�#���  &   System.Windows.Controls.DataGrid  -     Name$ 	SkinsGrid��$��1��   AutoGenerateColumns$ False��   CanUserAddRows$ False��	   CanUserDeleteRows$	 False��
   
IsReadOnly$
 False��$��0,0,0,10q�"   VerticalScrollBarVisibility$
 Auto=�$   HorizontalScrollBarVisibility$
 Auto=�  MaterialDesignDataGrid#��[    Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ 	Skin File��   
IsReadOnly$
 True�� & "MaterialDesignDataGridColumnHeader   HeaderStyle# [ 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *
FileName+4  .System.Windows.Controls.DataGridTemplateColumn  $ 
Skin Value��# [    CellTemplate �� .R��� $
��Wrap=����� *	SkinValue+   CellEditingTemplate �� .R���  �� AcceptsReturn$
 True��" �� VerticalScrollBarVisibility$
 Auto=� �� TextWrapping$
 Wrap=����� *	SkinValue+ �� UpdateSourceTrigger$ PropertyChanged=�   RowStyle ��  �� 
TargetType)  #System.Windows.Controls.DataGridRow"  .���  �� Property � ��  MaterialDesignBody �� Value# �   �� Triggers ��  �� Binding �� *
IsTeamBlue+ �� Value True.a���  � �� �	#993A5FCD��  �� *
IsTeamBlue+	 False.a���  � �� �	#99B22222��  �� *
IsTeamBlue+ Z� .a���  � ��  MaterialDesignPaper# �  �� $��2�� �� Orientation$ 
Horizontal=�$��Right=�$��0,10,0,0q�.+��� -     ��Name$  BtnSave��$
��Save��  MaterialDesignRaisedButton#��[ $	��100��$��0,0,10,0q��� -   $  BtnReset��$��Reset��#��[ $	��100��