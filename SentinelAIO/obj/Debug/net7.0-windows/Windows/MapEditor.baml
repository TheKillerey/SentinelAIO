   M S B A M L   `   `   `  ����F  BSentinelAIO, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:SentinelAIO.WindowsSentinelAIO.Windows  5   	   #    SentinelAIO.Windows.MapEditor   5   	   W SPresentationCore, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   3local!clr-namespace:SentinelAIO.Windows     5   	     � Title$  	MapEditor��5   	   $
��1000��6   $
��1600��6)    � 	Resources 5	         ThemedButtonStyle �� 
TargetType �� Property �� Value  ThemedProgressBarStyle��%�  &	        &	 i    �� 5   
   " ��6*   .�5      ��  � �� �#FFFFFF6+   �� 5       � �� �#0c6a4e6+   �� 5       � �� �#348c846,   �� 5       � ��$ 2q�60   �� 5       � ��$ 5q�6'   �� 5       � ��$ 20(�6)   �� 5       � ��$ /Fonts/#JetBrainsMono)�6+   �� 5       � ��$
 Thin"�6+   5      �� 5   
   " �6/   .�5      ��  � �� �#348c846+   �� 5       � �� �#348c846,   �� 5       � ��$ 2q�60   �� 5       � �� �#04191a6+   �� 5       � ��$ 30��6'   �� 5       � ��$	 200��6&   5      5      .��5       �� $��3q�6   ��5!   
   �� 5"       �� 
StartPoint$	 0,0&�6"    �� EndPoint$	 1,1&�63   .��5#      �  � Color$ #054535��6    � Offset$	 0.0Q�6/   � 5$      $ #054535��6   $
 0.33Q�6/   � 5%      $ #0c6a4e��6   $
 0.66Q�6/   � 5&      $ #348c84��6   $	 1.0Q�6/   5'      ._�5)   
   � ���#04191a	 � RowDefinitions	 5*      �� 5+      $
��Auto �6    �� 5,      $��* �6    .+�5/      c� $��0��6   ���#04191a.@�50      �� -   5        #��[  50      $��Load Map��6D   6W   �� 51      -   5        #��[  51      $��Save Map��6D   6W   52      � 53      $��1��6   
 � ColumnDefinitions
 54      �� 55      $	��300 �6'   �� 56      $��* �6'   5:      5;      5<      5=      