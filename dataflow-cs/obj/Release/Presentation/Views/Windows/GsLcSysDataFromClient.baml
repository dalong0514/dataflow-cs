   M S B A M L   `   `   `  ���� F  Bdataflow-cs, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null_4clr-namespace:dataflow_cs.Presentation.Views.Windows&dataflow_cs.Presentation.Views.Windows  W0clr-namespace:dataflow_cs.Presentation.ViewModel"dataflow_cs.Presentation.ViewModel  B    <dataflow_cs.Presentation.Views.Windows.GsLcSysDataFromClient   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     Flocal4clr-namespace:dataflow_cs.Presentation.Views.Windows     ?vm0clr-namespace:dataflow_cs.Presentation.ViewModel     H DMahApps.Metro, Version=1.6.5.1, Culture=neutral, PublicKeyToken=null;controls,http://metro.mahapps.com/winfx/xaml/controls  =	iconPacks-http://metro.mahapps.com/winfx/xaml/iconpacks    � Title$  数智设计��$	��450��$	��265��(  "MahApps.Metro.Controls.MetroWindow   TitleCharacterCasing$ Normal=�$��1q�   AccentColorBrush#���    � WindowStartupLocation$ CenterScreen=�   	GlowBrush# �     
GrayBrush2   NonActiveGlowBrush# �   � 
ResizeMode$ CanMinimize=�   ShowIconOnTitleBar .     ShowCloseButton .    ShowMaxRestoreButton .  	   ShowMinButton	 . 
   SaveWindowPosition
 .     IconTemplate Z�  � 	Resources  �� MergedDictionaries �� Source  CustomMetroHeaderStyle �� 
TargetType(  "MahApps.Metro.Controls.MetroHeader �� Property �� Value��  �� $I Cpack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml?��� $F @pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml?��� $G Apack://application:,,,/MahApps.Metro;component/Styles/Colors.xaml?��� $M Gpack://application:,,,/MahApps.Metro;component/Styles/Accents/Blue.xaml?��� $R Lpack://application:,,,/MahApps.Metro;component/Styles/Accents/BaseLight.xaml?�%Y   &	       �� "  .���  � ��$ 16(���  � ��$
 Bold"���  � ��# �   .��� $��10q� � RowDefinitions �� $
��Auto ��� $��* ��� $
��Auto �.+��� $��0��$��0,0,0,10q�#���   $��0,0,0,1q�  $��1��$
��10,0q�$��数据同步选项��$��16(�$
��Bold"�  ControlBackgroundBrush#���  #���   .����   
GrayBrush5#���  $��1q� �� Padding$ 10q�  WindowBackgroundBrush#���  +  %MahApps.Metro.Controls.ControlsHelper   CornerRadius$ 3��$	��122��._��� .+�]� $��0,5,0,10q�.+��� $	��150��$��Center=�$
��Left=� �� TextAlignment$
 Left=�$��13(��� $�	选择要����  � Language$ zh-cn�$�同步���� $�的数据类型:���� -    ��Name$ DataTypeComboBox��$	��230��$
��Left=�*  $MahApps.Metro.Controls.TextBoxHelper   	Watermark$ 请选择数据类型��   UseFloatingWatermark . #���    �� DisplayMemberPath$
 Name������ *	DataTypes+ �� SelectedItem �� *SelectedDataType+]� $��0,5,0,10q�]� $��0,5,0,5q�.+��� $	��150��$��Center=�$
��Left=�$
 Left=�$��13(��� $ zh-cn�$�同步���� $�数据状态:���� -    ��Name$ DataStatusTextBlock��$	��250��$
��Left=�$��SemiBold"�#���   $��Center=����� *
DataStatus+� $��2��$��
10,15,10,5q�.+�A�  A� Columns$ 2��$
��Left=�.+��� -    ��Name$ SelectButton��$	��100��$��40��$
��50,0q�  AccentedSquareButtonStyle#���  ���	#CC119EDA.����  �� Orientation$ 
Horizontal=�.+�F� �������!��- �@KL ��- �P[; ��- ��- P[; ��- @KL ���- ��!�@KL @o@ �� �� ���!@o@�@o@��! �� �� �@o@@KL �@o@P[; ��2��- ��!��- ���!@KL ���!��!�@KL ��!�@KL @KL ���!@KL Ѐ�� @fр�[ @]� �Pq pٰ р�� �7� Ѱ$� `�s р�@T� 	  IdealForegroundColorBrush#���  $��18��$��18��  �� Stretch$  Uniform=�$��0,0,5,0q��� $��同步数据���� -   $ CancelButton��$	��100��$��40��  SquareButtonStyle#���  .���� $ 
Horizontal=�.+�F� |������!�a ��f@KL � � 0�� ��a @KL �@KL �a �0��  � �@KL �f��a ��!� � О� ��f��!���!�f�О�  � ���!�a 	 	 
GrayBrush1#��� 	 $��18��$��18��$  Uniform=�$��0,0,5,0q��� $��取消��