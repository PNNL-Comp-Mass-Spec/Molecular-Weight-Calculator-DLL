using System;
using MolecularWeightCalculator;
using ReactiveUI;
// ReSharper disable InconsistentNaming

namespace MolecularWeightCalculatorGUI.CapillaryFlowUI
{
    internal class EquationsViewModel : ReactiveObject
    {
        [Obsolete("For WPF design-time use only.", true)]
        public EquationsViewModel() : this(CapillaryType.PackedCapillary)
        {
        }

        public EquationsViewModel(CapillaryType capillaryType)
        {
            CapillaryType = capillaryType;
            if (CapillaryType == CapillaryType.OpenTubularCapillary)
            {
                ColumnVolumeEquation = ColumnVolumeOpenEquationTeX;
                LinearVelocityEquation = LinearVelocityOpenEquationTeX;
                VolumetricFlowRateEquation = VolumetricFlowRateOpenEquationTeX;
                LinearVelocityDescription = LinearVelocityDescriptionOpenTeX;
            }
            else
            {
                ColumnVolumeEquation = ColumnVolumePackedEquationTeX;
                LinearVelocityEquation = LinearVelocityPackedEquationTeX;
                VolumetricFlowRateEquation = VolumetricFlowRatePackedEquationTeX;
                LinearVelocityDescription = LinearVelocityDescriptionPackedTeX;
            }
        }

        public CapillaryType CapillaryType { get; }

        // LaTex-friendly syntax; not fully supported by WPFMath
        //private const string DeadTimeEquationTeX = @"Dead time=${\displaystyle\frac{L}{\text{Linear Velocity}}}$";
        //private const string ColumnVolumeOpenEquationTeX = @"Column volume=$L{\pi}r^2$";
        //private const string ColumnVolumePackedEquationTeX = @"Column volume=$L{\pi}r^2{\varepsilon}$";
        //private const string LinearVelocityOpenEquationTeX = @"Linear velocity=${\displaystyle\frac{Pr^2}{8{\eta}L}}$";
        //private const string LinearVelocityPackedEquationTeX = @"Linear velocity=${\displaystyle\frac{Pd_p^2{\varepsilon}^2}{180{\eta}L(1-{\varepsilon})^2}}$";
        //private const string VolumetricFlowRateOpenEquationTeX = @"Vol Flow Rate=(Linear Velocity)(${\pi}r^2$)=${\displaystyle\frac{Pr^2{\pi}r^2}{8{\eta}L}}$";
        //private const string VolumetricFlowRatePackedEquationTeX = @"Vol Flow Rate=(Linear Velocity)(${\pi}r^2$)($\varepsilon$)=${\displaystyle\frac{Pd_p^2{\varepsilon}^2{\pi}r^2}{180{\eta}L(1-{\varepsilon})^2}}\varepsilon$";
        //private const string LinearVelocityDescriptionOpenTeX = @"where $P$ is pressure (dynes/cm$^2$), $r$ is the column radius (cm), $\eta$ (eta) is viscosity (poise), and $L$ is column length (cm)";
        //private const string LinearVelocityDescriptionPackedTeX = @"where $P$ is pressure (dynes/cm$^2$), $d_p$ is the diameter of the packing particles (cm), $\varepsilon$ (epsilon) is interparticle porosity, $r$ is the column radius (cm), $\eta$ (eta) is viscosity (poise), and $L$ is column length (cm)";
        //private const string BroadeningEquationDescriptionStartTeX = @"The theoretical amount of extra-column broadening that results from flow in open tubes is computed using the concept of plate height, H=$\sigma_\mathrm{L}^2$/L, where $\sigma_\mathrm{L}^2$ is the spatial variance of the peak (cm$^2$) and L is the open tube length (cm). However, it is more useful to think in terms of temporal variance, $\sigma_\mathrm{t}^2$ (sec$^2$), which relates to spatial variance by $\sigma_\mathrm{L}^2/\mathrm{u}^2=\sigma_\mathrm{t}^2$ where u is linear velocity (cm/sec):";
        //private const string BroadeningEquationBasicTeX = @"H=A+B+C";
        //private const string BroadeningEquationBasicABTeX = @"A=0 and B=0 in open tubes";
        //private const string BroadeningEquationBasicCTeX = @"C=d$_\mathrm{c}^2$u/(96D$_\mathrm{m}$)";
        //private const string BroadeningEquationBasicDTeX = @"D$_\mathrm{m}$=diffusion coefficient of analyte=$10^{-6}$";
        //private const string BroadeningEquationExpandedTeX = @"H=d$_\mathrm{c}^2$u/(96D$_\mathrm{m}$)=$\sigma_\mathrm{L}^2/$L";
        //private const string BroadeningEquationReducedTeX = @"d$_\mathrm{c}^2$uL/(96D$_\mathrm{m}$)=$\sigma_\mathrm{L}^2$";
        //private const string BroadeningEquationSubstituteDescriptionTeX = @"Substituting $\sigma_\mathml{L}^2/\mathrm{u}^2=\sigma_\mathrm{t}^2$ and $\sigma_\mathrm{L}^2=\sigma_\mathrm{t}^2\mathrm{u}^2$";
        //private const string BroadeningEquationSubstitutedTeX = @"d$_\mathrm{c}^2$uL/(96D$_\mathrm{m}$)=$\sigma_\mathrm{t}^2\mathrm{u}^2$";
        //private const string BroadeningEquationSigmaT2SolveDescriptionTeX = @"Solving for $\sigma_\mathrm{t}^2$";
        //private const string BroadeningEquationSigmaT2SolveTeX = @"$\sigma_\mathrm{t}^2$=d$_\mathrm{c}^2$L/(96D$_\mathrm{m}$u)";
        //private const string BroadeningEquationFinalTeX = @"The width at the base of a peak is $4\sigma_\mathrm{t}$ so a 30 second wide peak would have $\sigma_\mathrm{t}$=7.5 seconds and $\sigma_\mathrm{t}^2$=56 sec$^2$";

        // Syntax notes: '\;' is shorthand for "\thickspace", '\,' is shorthand for "\thinspace", '\\' inserts a newline
        private const string Super2 = "\u00b2";
        private const string DeadTimeEquationTeX = @"\mathrm{Dead\,time=}{\frac{L}{\mathrm{Linear\,Velocity}}}";
        private const string ColumnVolumeOpenEquationTeX = @"\mathrm{Column\,volume=}L{\pi}r^2";
        private const string ColumnVolumePackedEquationTeX = @"\mathrm{Column\,volume=}L{\pi}r^2{\varepsilon}";
        private const string LinearVelocityOpenEquationTeX = @"\mathrm{Linear\,velocity}={\frac{Pr^2}{8{\eta}L}}";
        private const string LinearVelocityPackedEquationTeX = @"\mathrm{Linear\,velocity}={\frac{Pd_p^{\;\;2}{\varepsilon}^2}{180{\eta}L(1-{\varepsilon})^2}}";
        private const string VolumetricFlowRateOpenEquationTeX = @"\mathrm{Vol\,Flow\,Rate=(Linear\,Velocity)}({\pi}r^2)\\\text{                   }={\frac{Pr^2{\pi}r^2}{8{\eta}L}}";
        private const string VolumetricFlowRatePackedEquationTeX = @"\mathrm{Vol\,Flow\,Rate=(Linear\,Velocity)}({\pi}r^2)(\varepsilon)\\\text{                   }={\frac{Pd_p^{\;\;2}{\varepsilon}^2{\pi}r^2}{180{\eta}L(1-{\varepsilon})^2}}\varepsilon";
        private const string LinearVelocityDescriptionOpenTeX = @"\text{where }P\;\text{is pressure (dynes/cm" + Super2 + @"),}\\r\text{ is the column radius (cm),}\\\eta\text{ (eta) is viscosity (poise) and}\\L\text{ is column length (cm)}";
        private const string LinearVelocityDescriptionPackedTeX = @"\text{where }P\;\text{is pressure (dynes/cm" + Super2 + @"),}\\d_p\text{ is the diameter of the packing particles (cm),}\\\varepsilon\text{ (epsilon) is interparticle porosity, }r\text{ is the column radius (cm),}\\\eta\text{ (eta) is viscosity (poise) and }L\text{ is column length (cm)}";
        private const string BroadeningEquationDescriptionStartTeX = @"\text{The theoretical amount of extra-column broadening that results from flow in open}\\\text{tubes is computed using the concept of plate height, H=}\sigma_{\mathrm{L}}^{\;\;2}/\mathrm{L}\text{, where }\sigma_{\mathrm{L}}^{\;\;2}\text{ is the spatial}\\\text{variance of the peak (cm" + Super2 + @") and }\mathrm{L}\text{ is the open tube length (cm). However, it is more useful to}\\\text{think in terms of temporal variance, }\sigma_{\mathrm{t}}^{\;\;2}\text{ (sec" + Super2 + @"), which relates to spatial variance by}\\\sigma_{\mathrm{L}}^{\;\;2}/\mathrm{u}^2=\sigma_{\mathrm{t}}^{\;\;2}\text{ where }\mathrm{u}\text{ is linear velocity (cm/sec):}";
        private const string BroadeningEquationBasicTeX = @"\mathrm{H=A+B+C}";
        private const string BroadeningEquationBasicABTeX = @"\mathrm{A=0}\text{ and }\mathrm{B=0}\text{ in open tubes}";
        private const string BroadeningEquationBasicCTeX = @"\mathrm{C=d_c^{\;\;2}u/(96D_m)}";
        private const string BroadeningEquationBasicDTeX = @"\mathrm{D_m}=\text{diffusion coefficient of analyte}=10^{-6}";
        private const string BroadeningEquationExpandedTeX = @"\mathrm{H=d_c^{\;\;2}u/(96D_m)=\sigma_L^{\;\;2}/L}";
        private const string BroadeningEquationReducedTeX = @"\mathrm{d_c^{\;\;2}u\,L/(96D_m)=\sigma_L^{\;\;2}}";
        private const string BroadeningEquationSubstituteDescriptionTeX = @"\text{Substituting }\mathrm{\sigma_L^{\;\;2}/u^2=\sigma_t^{\;\;2}}\text{ and }\mathrm{\sigma_L^{\;\;2}=\sigma_t^{\;\;2}u^2}";
        private const string BroadeningEquationSubstitutedTeX = @"\mathrm{d_c^{\;\;2}u\,L/(96D_m)=\sigma_t^{\;\;2}u^2}";
        private const string BroadeningEquationSigmaT2SolveDescriptionTeX = @"\text{Solving for }\mathrm{\sigma_t^{\;\;2}}";
        private const string BroadeningEquationSigmaT2SolveTeX = @"\mathrm{\sigma_t^{\;\;2}=d_c^{\;\;2}L/(96D_mu)}";
        private const string BroadeningEquationFinalTeX = @"\text{The width at the base of a peak is }\mathrm{4\sigma_t}\text{ so a 30 second wide peak would have}\\\mathrm{\sigma_t=7.5}\text{ seconds and }\mathrm{\sigma_t^{\;\;2}=56}\text{ sec" + Super2 + @"}";

        public string DeadTimeEquation => DeadTimeEquationTeX;
        public string ColumnVolumeEquation { get; }
        public string LinearVelocityEquation { get; }
        public string VolumetricFlowRateEquation { get; }
        public string LinearVelocityDescription { get; }
        public string BroadeningEquationDescriptionStart => BroadeningEquationDescriptionStartTeX;
        public string BroadeningEquationBasic => BroadeningEquationBasicTeX;
        public string BroadeningEquationBasicAB => BroadeningEquationBasicABTeX;
        public string BroadeningEquationBasicC => BroadeningEquationBasicCTeX;
        public string BroadeningEquationBasicD => BroadeningEquationBasicDTeX;
        public string BroadeningEquationExpanded => BroadeningEquationExpandedTeX;
        public string BroadeningEquationReduced => BroadeningEquationReducedTeX;
        public string BroadeningEquationSubstituteDescription => BroadeningEquationSubstituteDescriptionTeX;
        public string BroadeningEquationSubstituted => BroadeningEquationSubstitutedTeX;
        public string BroadeningEquationSigmaT2SolveDescription => BroadeningEquationSigmaT2SolveDescriptionTeX;
        public string BroadeningEquationSigmaT2Solve => BroadeningEquationSigmaT2SolveTeX;
        public string BroadeningEquationFinal => BroadeningEquationFinalTeX;
    }
}
