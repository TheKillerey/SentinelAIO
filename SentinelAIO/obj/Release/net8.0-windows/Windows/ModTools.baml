   M S B A M L   `   `   `  ����   SentinelAIO"    SentinelAIO.Windows.ModTools   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     c _MaterialDesignThemes.MahApps, Version=3.1.0.0, Culture=neutral, PublicKeyToken=df2a72020bd7962a_ [MaterialDesignThemes.Wpf, Version=5.0.1.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aHmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes   <d2http://schemas.microsoft.com/expression/blend/2008   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     _ [Microsoft.Xaml.Behaviors, Version=1.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a3b+http://schemas.microsoft.com/xaml/behaviors    � Title$  SentinelAIO - Mod Tools��$	��750��$
��1050��    MaterialDesignDarkForeground#���   #���   $��Medium"�$��14(���:  4MaterialDesignThemes.Wpf.MaterialDesignFontExtension   � 	Resources  �� 
TargetType   MaterialDesignFlatDarkButton �� BasedOn �� Property �� Value �� ScaleX �� ScaleY �� Triggers	 P� Property
 P� Value��%�   '��       7  �� " ��8   .���  �@J�RenderTransformOrigin$ 0.5,0.5&���  � z� �� $ 1Q�$ 1Q� P� 	 �@J�IsMouseOver$

 True��. ���  � z� �� $	 1.1Q�$	 1.1Q�.��� .+�#  MaterialDesignThemes.Wpf.Card  .���� 3  -MaterialDesignThemes.Wpf.NavigationRailAssist   ShowSelectionBackground$
 True�� J� SnapsToDevicePixels$
 True�� * &MaterialDesignNavigationRailTabControl#��[  �� TabStripPlacement$
 Left=�   FloatingContent �� -   $��	GoHomeBtn��$��10q� * &MaterialDesignFloatingActionDarkButton#��[ ��'  !MaterialDesignThemes.Wpf.PackIcon  $��40��$��40��   Kind$
 Home=�.@��� ���� $
��auto��$
��auto��.+�  $��24��$��24��$��Center=�$ Palette=���  �� TextAlignment$ Center=�$��	Mod Tools��.��� $��20q� � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��* �.+��� $��0��$��10q�  PrimaryHueDarkBrush#���   �� CornerRadius$ 10�� J� Effect 3  -System.Windows.Media.Effects.DropShadowEffect     
BlurRadius$ 10Q�   ShadowDepth$ 3Q�   Color$ Black��   Opacity$	 0.5Q�._�� $��20q� �� $
��Auto ��� $
��Auto � � ColumnDefinitions �� $��* ��� $��* ��� $��* ��� $��* �   CustomButtonStyle + 'MaterialDesign.Brush.Primary.Foreground  PrimaryHueMidBrush��%�   &	       7  �� " ��8   .���  � �� �	#7F2E2E2E��  � ��# �  ��  � ��$ 0q���  �@�Cursor$
 Hand�� P� 	 �@J�IsMouseOver$

 True��. ���  � ��# �  ��  � ��
 �����.+��� $��0��$��0��$��10q�$	��120��$	��100��
����� �$��2q�#��[  � ToolTip$O ILoad your Map folder and recolor your Map using .cube (LUT) / .png (HALD)������ *OpenLUTFileCommand+.����  �� Orientation$ Vertical=�$��Center=�.+�  $��50��$��50��$ Palette=�$��Center=��� $��PROJECT Terrain Shift��$��Center=�$
��Bold"��� $��Recoloring the map��$��Center=�$��10(�$��Light"�  $��0,0,0,0q�$��15��$��15��$ FolderAlert=�$��Center=�
������$  Map Project Folder needed!���� $��0��$��1��$��10q�$	��120��$	��100��
����� �$��2q�#��[ $&  Creates templates for every map.������ *GetMapFilesCommand+.���� $ Vertical=�$��Center=�.+�  $��50��$��50��$ HammerWrench=�$��Center=��� $��PROJECT Architect��$��Center=�$
��Bold"��� $ ��Creating map mod templates��$��Center=�$��10(�$��Light"�  $��0,0,0,0q�$��15��$��15��$ FolderAlert=�$��Center=�
������$  Map Project Folder needed!���� $��0��$��2��$
{�True��$��10q�$	��120��$	��100��
����� �$��2q�#��[ $M GSet your project to the default map. Example: crepe.mapgeo to base_srx.������ *SetDefaultMapCommand+.���� $ Vertical=�$��Center=�.+�  $��50��$��50��$	 Map=�$��Center=��� $��Core Simulation��$��Center=�$
��Bold"��� $��Default map environment��$��Center=�$��10(�$��Light"�  $��0,0,0,0q�$��15��$��15��$ FolderAlert=�$��Center=�
������$  Map Project Folder needed!���� $��0��$��3��${�False��$��10q�$	��120��$	��100��
����� �$��2q�#��[ $) #Set all of your minions to default.������ *SetDefaultMinionsCommand+.���� $ Vertical=�$��Center=�.+�  $��50��$��50��$ Robot=�$��Center=��� $��Synthwave Infantry��$��Center=�$
��Bold"��� $��Default Minion Skins��$��Center=�$��10(�$��Light"�  $��0,0,0,0q�$��15��$��15��$ FolderAlert=�$��Center=�
������$  Map Project Folder needed!���� $��1��$��0��$��10q�$	��120��$	��100��
����� �$��2q�#��[ $k eIf you have a raw particle file, it will extract the related files from the League of Legends folder.������ *RitoPatchFixerCommand+.���� $ Vertical=�$��Center=�.+�  $��50��$��50��$ DatabaseSearch=�$��Center=��� $��PROJECT Fixer��$ Center=�$��Center=�$
��Bold"��� $��Rey's Rito Patch Fixer��$��Center=�$��10(�$��Light"�  $��0,0,0,0q�$��15��$��15��$ FolderAlert=�$��Center=�
������$  Map Project Folder needed!���� $��1��$��1��$��10q�$	��120��$	��100��
����� �$��2q�#��[ $* $Fixes problems with DDS Compression.������ *FixImageFilesCommand+.���� $ Vertical=�$��Center=�.+�  $��50��$��50��$ Tools=�$��Center=��� $��Protocol: Asset Repair��$ Center=�$��Center=�$
��Bold"��� $!��Fixing Rito's images (.dds)��$��Center=�$��10(�$��Light"�  $��0,0,0,0q�$��15��$��15��$ FolderAlert=�$��Center=�
������$  Map Project Folder needed!���� $��1��$��2��$��10q�$	��120��$	��100��
����� �$��2q�#��[ $B <Assists in selecting color values for your map or particles.������ * RemoveNotNeededTexturesCommand+.���� $ Vertical=�$��Center=�.+�  $��50��$��50��$ RemoveCircle=�$��Center=��� $��Rift System Cleanup��$��Center=�$ Center=�$
��Bold"��� $ ��Remove not needed textures��$��Center=�$��10(�$��Light"�  $��0,0,0,0q�$��15��$��15��$ FolderAlert=�$��Center=�
������$  Map Project Folder needed!���� $��1��$��3��$��10q�$	��120��$	��100��
����� �$��2q�#��[ $0 *Assists in editing the Map Materials file.������ *MapMaterialEditor+.���� $ Vertical=�$��Center=�.+�  $��50��$��50��$ 	MapLegend=�$��Center=��� $��Rift System Editor��$��Center=�$ Center=�$
��Bold"��� $��Edit Map Materials File��$��Center=�$��10(�$��Light"�  $��0,0,0,0q�$��15��$��15��$ FolderAlert=�$��Center=�
������$  Map Project Folder needed!��� $��1��$��0q� �� $
��Auto ��� $
��Auto ��� $
��Auto � �� $��3* ��� $��1* �.+��� $��0��$��0��$��2��$��	0,10,0,10q�$
��Left=�.+�  $��0,0,0,0q�$��35��$��35��$ Folder=�$
��Left=�
�������� $��Add Map Project Folder��$��16(�$
��Bold"�#���  �� $@��:(Correct: D:\Mods\OldRift\ | Wrong: D:\Mods\OldRift\Map11)��$��12(�$��
UltraLight"�#���  �� -   $��ProjectFolder��$��1��$��0��$��5q�$��Center=�$��40��  MaterialDesignTextBox#��[ ���� *ProjectFolderPath+ �� UpdateSourceTrigger$ PropertyChanged=��� $��1��$��1��$��5q�$��Center=�$��30��$	��100��$	��Add�� 	 MaterialDesignFlatButton#��[	 $3 -Add the project folder to your configuration.������ *AddMapCommand+�� $��2��$��0��$
��Left=�.+��� -    ��Name$ UpdateHashesText�� 
 MaterialDesignTextBlock#��[
 ���� *CurrentFileInfo+1  +System.Diagnostics.PresentationTraceSources   
TraceLevel$
 High=�y��� *VisibilityProgressbar+� -    �Name$ DownloadProgressBar��$��10��$	��200��$
��Left=� # MaterialDesignLinearProgressBar#��[  � Value �� *CurrentProgress+  � Maximum  �� *
TotalFiles+! � IsIndeterminate! �� *IsIndeterminate+y��� *VisibilityProgressbar+�� $
��Left=�$��5,5,0,0q�$��14(�#���  ���� *ProgressBarPercentage+" �� StringFormat$" {0:F2}%��y��� *VisibilityProgressbar+�� ${�False������ $
��auto��$
��auto��.+�  $��24��$��24��$��Center=�$ FileTree=��� $ Center=�$��Mesh
Editor��.��� $��16q� �� $
��Auto ��� $��* � �� $��* ��� $��2* �.+��� $ 
Horizontal=�$��0��$��2��$��Right=�$��0,8,0,16q�.+��� $��Import��$��4q�$	��100��#��[	 $&  Import new meshes into the tree.������ *OpenMapGeoCommand+�� $��Export��$��4q�$	��100��#��[	 $/ )Export mesh settings to an external file.������ *ExportCommand+�� $��1��$��0��$��16q�$
��Auto=�$
��Auto=�  MaterialDesignPaper#���  .��R� -    ) %MaterialDesignDarkSeparatorBackground#���  ���� *	MeshNames+���� .R��� ���� �� $��1��$��1��$��16q�.+��� $��Mesh Settings��$��16(�$
��Bold"�#���  $��0,0,0,10q��� $��Name:��
��������� $��40��$��0,5,0,10q�  MaterialDesignTextBoxBase#��[ ���� *SelectedMeshName+# �� Mode$# TwoWay=��� $��	Position:��
��������� $ 
Horizontal=�$��0,5,0,10q�.+��� $��X:��
�������$��0,0,5,0q��� $��50��$��40��$��0,0,10,0q�#��[ ���� *SelectedMeshPositionX+�� $��Y:��
�������$��0,0,5,0q��� $��50��$��40��$��0,0,10,0q�#��[ ���� *SelectedMeshPositionY+�� $��Z:��
�������$��0,0,5,0q��� $��50��$��40��$��0,0,10,0q�#��[ ���� *SelectedMeshPositionZ+�� $��Scale:��
��������� $ 
Horizontal=�$��0,5,0,10q�.+��� $��X:��
�������$��0,0,5,0q��� $��50��$��40��$��0,0,10,0q�#��[ ���� *SelectedMeshScaleX+�� $��Y:��
�������$��0,0,5,0q��� $��50��$��40��$��0,0,10,0q�#��[ ���� *SelectedMeshScaleY+�� $��Z:��
�������$��0,0,5,0q��� $��50��$��40��$��0,0,10,0q�#��[ ���� *SelectedMeshScaleZ+�� $��	Rotation:��
��������� $ 
Horizontal=�$��0,5,0,10q�.+��� $��X:��
�������$��0,0,5,0q��� $��50��$��40��$��0,0,10,0q�#��[ ���� *SelectedMeshRotationX+�� $��Y:��
�������$��0,0,5,0q��� $��50��$��40��$��0,0,10,0q�#��[ ���� *SelectedMeshRotationY+�� $��Z:��
�������$��0,0,5,0q��� $��50��$��40��$��0,0,10,0q�#��[ ���� *SelectedMeshRotationZ+�� ${�False������ $
��auto��$
��auto��.+�  $��24��$��24��$��Center=�$ Update=��� $ Center=�$��Mod
Updater��.���  �� $
��Auto ��� .+��� $��0��$��10q�#��[ $
��Left=�$ Load Mod File������ *OpenModFileCommand+.��  $��30��$��30��$
 Plus=��� $��1��$
��Auto=�$��10q�.���� ���� *
ModInfos+���� .R��� $��5q�$ �� Padding$$ 4q�#���  $ 8��._��  �� $
��Auto ��� $��* � �� $	��170 ��� .+��� $ Vertical=�$��0��$	��Top=�.+��� $	��100��$	��193��$��Uniform=�$��10q�$��Center=����� *
RouteImage+   $ Black��$ 10Q�$ 5Q�$	 0.5Q��� $ Vertical=�$	��293��$��60��$��0,-5,0,0q�$��Center=�.+��� $
��Bold"�$��12(�
�������$ Center=�$
��Wrap=�$��Center=�$��Center=����� *Name+�� $��Medium"�$��10(�
�������$ Center=�$
��Wrap=�$��Center=�$��Center=����� *Author+�� $��Light"�$��10(�
�������$ Center=�$
��Wrap=�$��Center=�$��Center=����� *	Version+�� $ 
Horizontal=�$��1��$��Center=�$
��Left=�$��20,0,0,0q�.+��� #��[ $��10q�$ View mod description������ *ShowDescriptionCommand+.��  $ InformationOutline=�$��30��$��30���� #��[ $��10q�$" Add or remove from favorites������ *ToggleHeartCommand+.��  $ HeartOutline=�$��30��$��30���� #��[ $��10q�$ Visit the mod's home page������ *OpenHomePageCommand+.��  $ HomeOutline=�$��30��$��30���� #��[ $��10q�$ Update the mod files������ *UpdateModCommand+���� .��  $ Update=�$��30��$��30���� #��[ $��10q�$! Send this mod to CS-Manager������ *SendToCSManagerCommand+.��  $
 Send=�$��30��$��30���� $
{�True������ $
��auto��$
��auto��.+�  $��24��$��24��$��Center=�$ Matrix=��� $ Center=�$��Matrix
Helper��.���  �� $
��Auto ��� $��* �.+�� $��0��#���   �� $
��Auto ��� $
��Auto ��� $��* ��� $��50 � �� $
��Auto ��� $
��Auto ��� $��* �.+��� $ Vertical=�$	��600��$��0��$��0��$��	10,10,0,0q�$
��Left=�$	��Top=�.+��� $��Maya Translate:��
�������$��0,0,0,10q��� $ 
Horizontal=�.+��� $��X:��
�������$��0,0,10,0q��� -   % ��Name$% TransX��$	��150��$��40��#���  $��	0,0,10,10q����� *TransXInput+$" F8���� $��Y:��
�������$��0,0,10,0q��� -   $% TransY��$	��150��$��40��#��[ $��	0,0,10,10q����� *TransYInput+$" F8���� $��Z:��
�������$��0,0,10,0q��� -   $% TransZ��$	��150��$��40��#��[ $��	0,0,10,10q����� *TransZInput+$" F8���� $��Maya Scale:��
�������$��0,0,0,10q��� $ 
Horizontal=�.+��� $��X:��
�������$��0,0,10,0q��� -	   $% ScaleX��$	��150��$��40��#��[ $��	0,0,10,10q����� *ScaleXInput+$" F8���� $��Y:��
�������$��0,0,10,0q��� -
   $% ScaleY��$	��150��$��40��#��[ $��	0,0,10,10q����� *ScaleYInput+$" F8���� $��Z:��
�������$��0,0,10,0q��� -   $% ScaleZ��$	��150��$��40��#��[ $��	0,0,10,10q����� *ScaleZInput+$" F8���� $��Maya Rotation:��
�������$��0,0,0,10q��� $ 
Horizontal=�.+��� $��X:��
�������$��0,0,10,0q��� -   $
% RotX��$	��150��$��40��#��[ $��	0,0,10,10q����� *	RotXInput+$" F8���� $��Y:��
�������$��0,0,10,0q��� -   $
% RotY��$	��150��$��40��#��[ $��	0,0,10,10q����� *	RotYInput+$" F8���� $��Z:��
�������$��0,0,10,0q��� -   $
% RotZ��$	��150��$��40��#��[ $��	0,0,10,10q����� *	RotZInput+$" F8���� $ Vertical=�$��1��$��0��$
��Left=�$	��Top=�$��
10,10,00,0q�.+��� $ ��League of Legends Matrix44��
�������$��	00,0,0,10q��  �� �� �� ��  �� �� �� �� .+��� $ 
Horizontal=�$��0��.+��� $
��M11:��
�������$��0,0,10,0q��� -   $	% M11��$	��150��$��40��#��[ $��
10,0,10,10q����� *
M11Input+$" F8���� $
��M12:��
�������$��0,0,10,0q��� -   $	% M12��$	��150��$��40��#��[ $��
10,0,10,10q����� *
M12Input+$" F8���� $
��M13:��
�������$��0,0,10,0q��� -   $	% M13��$	��150��$��40��#��[ $��
10,0,10,10q����� *
M13Input+$" F8���� $
��M14:��
�������$��0,0,10,0q��� -   $	% M14��$	��150��$��40��#��[ $��
10,0,10,10q����� *
M14Input+$" F8���� $ 
Horizontal=�$��1��.+��� $
��M21:��
�������$��0,0,10,0q��� -   $	% M21��$	��150��$��40��#��[ $��
10,0,10,10q����� *
M21Input+$" F8���� $
��M22:��
�������$��0,0,10,0q��� -   $	% M22��$	��150��$��40��#��[ $��
10,0,10,10q����� *
M22Input+$" F8���� $
��M23:��
�������$��0,0,10,0q��� -   $	% M23��$	��150��$��40��#��[ $��
10,0,10,10q����� *
M23Input+$" F8���� $
��M24:��
�������$��0,0,10,0q��� -   $	% M24��$	��150��$��40��#��[ $��
10,0,10,10q����� *
M24Input+$" F8���� $ 
Horizontal=�$��2��.+��� $
��M31:��
�������$��0,0,10,0q��� -   $	% M31��$	��150��$��40��#��[ $��
10,0,10,10q����� *
M31Input+$" F8���� $
��M32:��
�������$��0,0,10,0q��� -   $	% M32��$	��150��$��40��#��[ $��
10,0,10,10q����� *
M32Input+$" F8���� $
��M33:��
�������$��0,0,10,0q��� -   $	% M33��$	��150��$��40��#��[ $��
10,0,10,10q����� *
M33Input+$" F8���� $
��M34:��
�������$��0,0,10,0q��� -   $	% M34��$	��150��$��40��#��[ $��
10,0,10,10q����� *
M34Input+$" F8���� $ 
Horizontal=�$��3��.+��� $
��M41:��
�������$��0,0,10,0q��� -   $	% M41��$	��150��$��40��#��[ $��
10,0,10,10q����� *
M41Input+$" F8���� $
��M42:��
�������$��0,0,10,0q��� -   $	% M42��$	��150��$��40��#��[ $��
10,0,10,10q����� *
M42Input+$" F8���� $
��M43:��
�������$��0,0,10,0q��� -   $	% M43��$	��150��$��40��#��[ $��
10,0,10,10q����� *
M43Input+$" F8���� $
��M44:��
�������$��0,0,10,0q��� -   $	% M44��$	��150��$��40��#��[ $��
10,0,10,10q����� *
M44Input+$" F8��� $��1�� �� $
��Auto ��� $
��Auto �.+��� $
��Left=�$��	10,28,0,0q�$��0��$��0��$
��Wrap=�$5��/Paste your League Matrix here same as the hint:��$	��Top=��� $��0��$��0��$	��140��$	��280��)  #MaterialDesignThemes.Wpf.HintAssist&   Hint$c& ]transform: mtx44 = {
1, 0, 0, 0
0, 1, 0, 0
0, 0, 1, 0
1827.10999, 149.139999, 6302.22021, 1
}��' �� AcceptsReturn$
' True��( �� 	IsEnabled$( False��$�� ��) �� TextWrapping$
) Wrap=�"* �� VerticalScrollBarVisibility$
* Auto=�$��	10,48,0,0q�$	��Top=�$
��Left=�#��[ $	��377��.��*  $Microsoft.Xaml.Behaviors.Interaction+   Triggers+ +	  %Microsoft.Xaml.Behaviors.EventTrigger	  , 	  	EventName$, TextChanged��*
  $Microsoft.Xaml.Behaviors.TriggerBase- 
  Actions.- 2  ,Microsoft.Xaml.Behaviors.InvokeCommandAction  .   Command. �� *UpdateMatrixCommand+/   CommandParameter/ �� *Text+0 �� RelativeSource0 �� 1 �� Mode$1 FindAncestor=�2 �� AncestorType2 TextBox.���� $
��Left=�$��	10,28,0,0q�$��0��$��1��$
��Wrap=�$��Output:��$	��Top=��� -   $��OutputTb��$��0��$��1��$	��140��$	��280��$& 
Copy this!��$
' True��$( False��$
) Wrap=�$
* Auto=�$��	10,48,0,0q�$	��Top=�$
��Left=�#��[ 3 �� 
IsReadOnly$
3 True��$	��377������ *
MatrixData+�� -    $��CopyBtn��$��1��$
��COPY��$
��Left=�$��392,159,0,0q�$��0��$	��Top=�$��29��$��92�� ! MaterialDesignPaperDarkButton#��[ 