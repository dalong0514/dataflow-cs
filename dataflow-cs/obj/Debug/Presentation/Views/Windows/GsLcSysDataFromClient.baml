   M S B A M L   `   `   `  ����F  Bdataflow-cs, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null_4clr-namespace:dataflow_cs.Presentation.Views.Windows&dataflow_cs.Presentation.Views.Windows  5   	   W0clr-namespace:dataflow_cs.Presentation.ViewModel"dataflow_cs.Presentation.ViewModel  5   	   B    <dataflow_cs.Presentation.Views.Windows.GsLcSysDataFromClient   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Flocal4clr-namespace:dataflow_cs.Presentation.Views.Windows     5   	   ?vm0clr-namespace:dataflow_cs.Presentation.ViewModel     5   	   H DMahApps.Metro, Version=1.6.5.1, Culture=neutral, PublicKeyToken=null;controls,http://metro.mahapps.com/winfx/xaml/controls  5   	   =	iconPacks-http://metro.mahapps.com/winfx/xaml/iconpacks  5	   	     � Title$'  !数智设计——同步CAD数据��5   	   $	��450��5   	   $	��250��5   	   (  "MahApps.Metro.Controls.MetroWindow   TitleCharacterCasing$ Normal=�5   	   $��1q�5   	      AccentColorBrush#���   5   	    � WindowStartupLocation$ CenterScreen=�5   	      	GlowBrush# �   5   	     
GrayBrush2   NonActiveGlowBrush# �  5   	    � 
ResizeMode$ CanMinimize=�5   	      ShowIconOnTitleBar .     ShowCloseButton .    ShowMaxRestoreButton .  	   ShowMinButton	 . 
   SaveWindowPosition
 .     IconTemplate 5   	   Z�  � 	Resources 5       �� MergedDictionaries �� Source  CustomMetroHeaderStyle �� 
TargetType(  "MahApps.Metro.Controls.MetroHeader �� Property �� Value�� 5   
    5      �� 5      $I Cpack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml?�6%   �� 5      $F @pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml?�6%   �� 5       $G Apack://application:,,,/MahApps.Metro;component/Styles/Colors.xaml?�6%   �� 5!      $M Gpack://application:,,,/MahApps.Metro;component/Styles/Accents/Blue.xaml?�6%   �� 5"      $R Lpack://application:,,,/MahApps.Metro;component/Styles/Accents/BaseLight.xaml?�6%   %�   &	       �� 5&      "  63   .�5'      ��  � ��$ 16(�6-   �� 5(       � ��$
 Bold"�6/   �� 5)       � ��# �   6/   5*      5+      .��5.      � $��10q�6    � RowDefinitions 5/   
   �� 50      $
��Auto �6   �� 51      $��* �6   �� 52      $
��Auto �6   .+�56   
   �� $��0��6   $��0,0,0,10q�6   #���   57      $��0,0,0,1q�58      ._�59      �� $��18(�5:      $
��Bold"�5;      #���   5<      $��5,0,0,5q�5=      �� 6*   $�同步��6.   �� 6:   $�	CAD数据��6>   6N   5>        5A   
   $��1��6   $
��10,0q�6,   $��数据同步选项��5B      $��16(�5C      $
��Bold"�5D        ControlBackgroundBrush#���  5E      #���   5F      .��5G      ��   
GrayBrush5#���  6   $��1q�5H      $��0,10,0,0q�5I       �� Padding$ 10q�5J        WindowBackgroundBrush#���  5K      +  %MahApps.Metro.Controls.ControlsHelper   CornerRadius$ 3��5L      ._�5M      �� .+�5O      ]� $��0,5,0,10q�6    .+�5P      �� $	��150��6$   $��Center=�5Q   !   $
��Left=�5R   !    �� TextAlignment$
 Left=�5S   !   $��13(�5T   !   �� 60   $�	选择要��64   �� 6A    � Language$ zh-cn�6E   $�同步��6V   �� 6b   $�的数据类型:��6f   6w   �� 5U      -   5         ��Name$ DataTypeComboBox��5U   #   $	��230��5V   !   $
��Left=�5W   !   *  $MahApps.Metro.Controls.TextBoxHelper   	Watermark$ 请选择数据类型��5X   !      UseFloatingWatermark . #���   5Z   !    �� DisplayMemberPath$
 Name��5]   !   ��5[   !   �� *	DataTypes+ �� SelectedItem 5\   !   �� *SelectedDataType+5]   !   5^      ]� 5a      $��0,5,0,10q�6    ]� 5d      $��0,5,0,5q�6    .+�5e      �� $	��150��6$   $��Center=�5f   !   $
��Left=�5g   !   $
 Left=�5h   !   $��13(�5i   !   �� 60   $ zh-cn�64   $�同步��6E   �� 6Q   $�数据状态:��6U   6e   �� 5j      -   5         ��Name$ DataStatusTextBlock��5j   $   $	��250��5l   !   $
��Left=�5m   !   $��SemiBold"�5n   !   #���   5o   !   $��Center=�5p   !   ��5k   !   �� *
DataStatus+5p   !   5q      5r      5s      5t      � 5w   
   $��2��6   $��
10,15,10,5q�6   .+�5x      A�  A� Columns$ 2��6   $
��Left=�6&   .+�5y      �� -   5         ��Name$ SelectButton��5y      $	��100��5z      $��40��6%   $
��50,0q�5{        AccentedSquareButtonStyle#���  5|      ���	#CC119EDA.��5~      ��  �� Orientation$ 
Horizontal=�6!   .+�5      F� �������!��- �@KL ��- �P[; ��- ��- P[; ��- @KL ���- ��!�@KL @o@ �� �� ���!@o@�@o@��! �� �� �@o@@KL �@o@P[; ��2��- ��!��- ���!@KL ���!��!�@KL ��!�@KL @KL ���!@KL Ѐ�� @fр�[ @]� �Pq pٰ р�� �7� Ѱ$� `�s р�@T� 	  IdealForegroundColorBrush#���  5�      $��18��5�      $��18��6&     �� Stretch$  Uniform=�5�      $��0,0,5,0q�5�      �� 5�      $��同步数据��6$   5�      5�      �� 5�      -   5        $ CancelButton��5�      $	��100��5�      $��40��6%     SquareButtonStyle#���  5�      .��5�      �� $ 
Horizontal=�6!   .+�5�      F� |������!�a ��f@KL � � 0�� ��a @KL �@KL �a �0��  � �@KL �f��a ��!� � О� ��f��!���!�f�О�  � ���!�a 	 	 
GrayBrush1#��� 	 5�      $��18��5�      $��18��6&   $  Uniform=�5�      $��0,0,5,0q�5�      �� 5�      $��取消��6$   5�      5�      5�      5�      5�      5�      