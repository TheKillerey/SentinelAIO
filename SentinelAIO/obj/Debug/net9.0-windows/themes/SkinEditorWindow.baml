   M S B A M L   `   `   `  ����  SentinelAIO)    #SentinelAIO.Themes.SkinEditorWindow   5   	   W SPresentationCore, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   c _MaterialDesignThemes.MahApps, Version=5.2.2.0, Culture=neutral, PublicKeyToken=df2a72020bd7962a_ [MaterialDesignThemes.Wpf, Version=5.2.2.0, Culture=neutral, PublicKeyToken=df2a72020bd7962a<md1http://materialdesigninxaml.net/winfx/xaml/themes   5   	     � Title$  Skin Editor��5   	   $	��500��5	   	   $	��800��5
   	    � WindowStartupLocation$ CenterScreen=�5   	       MaterialDesignDarkBackground#���   5   	      MaterialDesignDarkForeground#���  5   	   $��14(�5   	   ��5   	   :  4MaterialDesignThemes.Wpf.MaterialDesignFontExtension  .��5      � $��20q�6    � ColumnDefinitions 5   
   �� 5      $	��200 �6   �� 5      $��* �6   .+�5   
   � $��0��6   $��10q�6    � RowDefinitions 5      �� 5      $
��Auto �6    �� 5      $��* �6    .+�5      �� $��	Champions��6   $��16(�5      $��Medium"�5       $��0,0,0,10q�5!      #���  5"      �� 5$      -   5         ��Name$ ChampionsList��5$      $��1��5%        MaterialDesignListBox#��[ 5'      ��5(      �� 5)       �� 
TargetType" ��6&   .R�5*      �� $
��Auto=�6'   $
��Auto=�5+   '   .��5,      �� 5-      5.      50      51      � 54   
   $��1��6   $��10q�6    55      �� 56      $
��Auto �6    �� 57      $��* �6    �� 58      $
��Auto �6    .+�5;      �� $��Skins��6   $��16(�5<      $��Medium"�5=      $��0,0,0,10q�5>      #���  5?      &   System.Windows.Controls.DataGrid  5A      -   5          Name$ 	SkinsGrid��5A      $��1��5B         AutoGenerateColumns$ False��5D         CanUserAddRows$ False��5E      	   CanUserDeleteRows$	 False��5F      
   
IsReadOnly$
 False��5G      $��0,0,0,10q�5H      "   VerticalScrollBarVisibility$
 Auto=�5I      $   HorizontalScrollBarVisibility$
 Auto=�5J        MaterialDesignDataGrid#��[ 5K         Columns 5L      0  *System.Windows.Controls.DataGridTextColumn  5N      ,  &System.Windows.Controls.DataGridColumn   Header$ 	Skin File��6)      
IsReadOnly$
 True��5P   )    & "MaterialDesignDataGridColumnHeader   HeaderStyle# [ 5Q   )   1  +System.Windows.Controls.DataGridBoundColumn   Binding 5O   )   �� *
FileName+5Q   )   4  .System.Windows.Controls.DataGridTemplateColumn  5S      $ 
Skin Value��6-   # [ 5T   -      CellTemplate 5U      �� 5V      .R�5X   "   �� $
��Wrap=�5Y   ,   ��5X   ,   �� *	SkinValue+5Y   ,   5Z         CellEditingTemplate 5\      �� 5]      .R�5_   "   ��  �� AcceptsReturn$
 True��5`   *   " �� VerticalScrollBarVisibility$
 Auto=�5a   *    �� TextWrapping$
 Wrap=�5b   *   ��5_   *   �� *	SkinValue+ �� UpdateSourceTrigger$ PropertyChanged=�5b   *   5c      5e         RowStyle 5g      �� 5h       �� 
TargetType)  #System.Windows.Controls.DataGridRow"  6   .�5i      ��  �� Property � ��  MaterialDesignBody �� Value# �  67    �� Triggers 5k      �� 5l       �� Binding 6*   �� *
IsTeamBlue+ �� Value True6I   .a�5m   "   ��  � �� �	#993A5FCD6?   5n      �� 5o       6*   �� *
IsTeamBlue+	 False6I   .a�5p   "   ��  � �� �	#99B222226?   5q      �� 5r       6*   �� *
IsTeamBlue+ 6I   Z� .a�5s   "   ��  � ��  MaterialDesignPaper# �  6?   5t      5v      5x      �� 5{      $��2��6    �� Orientation$ 
Horizontal=�5|      $��Right=�5}      $��0,10,0,0q�5~      .+�5      �� -   5          ��Name$  BtnSave��5      $
��Save��5�        MaterialDesignRaisedButton#��[ 5�      $	��100��5�      $��0,0,10,0q�5�      �� 5�      -   5        $  BtnReset��5�      $��Reset��5�      #��[ 5�      $	��100��5�      5�      5�      5�      5�      