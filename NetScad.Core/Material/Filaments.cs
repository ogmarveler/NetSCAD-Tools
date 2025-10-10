using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetScad.Core.Material
{
    // Enum for 3D printing filament types
    public enum FilamentType
    {
        PLA,
        ABS,
        PETG,
        TPU,
        Nylon,
        ASA,
        PC,
        HIPS,
        PVA,
        CarbonFiber,
        ABS_GF,
        PLA_FR
    }

    // Record to store filament printing settings, electrical standards, and durability
    public record FilamentInfo
    {
        public FilamentType Type { get; init; }
        public int MinExtruderTemp { get; init; } // °C
        public int MaxExtruderTemp { get; init; } // °C
        public int MinBedTemp { get; init; }      // °C
        public int MaxBedTemp { get; init; }      // °C
        public string Cooling { get; init; }      // Cooling fan requirements
        public string UL94Rating { get; init; }   // UL94 flammability rating
        public bool RoHSCompliant { get; init; }  // RoHS compliance
        public int MaxWeightSupportKg { get; init; } // Max weight support in kg (stress-tested)
        public string Notes { get; init; }        // Printing, electrical, and durability considerations
    }

    // Extension methods for FilamentType
    public static class FilamentTypeExtensions
    {
        public static FilamentInfo GetInfo(this FilamentType type) => type switch
        {
            FilamentType.PLA => new FilamentInfo
            {
                Type = FilamentType.PLA,
                MinExtruderTemp = 190,
                MaxExtruderTemp = 220,
                MinBedTemp = 20,
                MaxBedTemp = 60,
                Cooling = "High (50-100%)",
                UL94Rating = "HB",
                RoHSCompliant = true,
                MaxWeightSupportKg = 50,
                Notes = "Easy to print, biodegradable, low warp. HB rating (low flame resistance); not ideal for high-heat electrical enclosures. Supports ~50 kg in stress tests; good for lightweight structural parts. RoHS compliant."
            },
            FilamentType.ABS => new FilamentInfo
            {
                Type = FilamentType.ABS,
                MinExtruderTemp = 220,
                MaxExtruderTemp = 250,
                MinBedTemp = 80,
                MaxBedTemp = 100,
                Cooling = "Low (0-30%)",
                UL94Rating = "HB",
                RoHSCompliant = true,
                MaxWeightSupportKg = 70,
                Notes = "Strong, durable. Requires enclosed printer due to fumes and warping. HB rating; use in well-ventilated areas for electrical components. Supports ~70 kg; suitable for moderately loaded parts. RoHS compliant."
            },
            FilamentType.PETG => new FilamentInfo
            {
                Type = FilamentType.PETG,
                MinExtruderTemp = 230,
                MaxExtruderTemp = 250,
                MinBedTemp = 70,
                MaxBedTemp = 80,
                Cooling = "Medium (30-60%)",
                UL94Rating = "HB",
                RoHSCompliant = true,
                MaxWeightSupportKg = 65,
                Notes = "Balances strength and ease of printing. Good for functional parts. HB rating; suitable for non-critical electrical casings. Supports ~65 kg; versatile for structural electrical components. RoHS compliant."
            },
            FilamentType.TPU => new FilamentInfo
            {
                Type = FilamentType.TPU,
                MinExtruderTemp = 220,
                MaxExtruderTemp = 240,
                MinBedTemp = 20,
                MaxBedTemp = 60,
                Cooling = "Low (0-30%)",
                UL94Rating = "HB",
                RoHSCompliant = true,
                MaxWeightSupportKg = 30,
                Notes = "Flexible, elastic. Use direct drive extruder. HB rating; limited use in electrical applications due to flexibility. Supports ~30 kg; best for flexible components, not heavy loads. RoHS compliant."
            },
            FilamentType.Nylon => new FilamentInfo
            {
                Type = FilamentType.Nylon,
                MinExtruderTemp = 240,
                MaxExtruderTemp = 260,
                MinBedTemp = 70,
                MaxBedTemp = 90,
                Cooling = "Low (0-20%)",
                UL94Rating = "V-2",
                RoHSCompliant = true,
                MaxWeightSupportKg = 85,
                Notes = "Strong, durable. Hygroscopic; dry before printing. V-2 rating (self-extinguishing); good for electrical housings. Supports ~85 kg; excellent for high-strength electrical parts. RoHS compliant."
            },
            FilamentType.ASA => new FilamentInfo
            {
                Type = FilamentType.ASA,
                MinExtruderTemp = 230,
                MaxExtruderTemp = 260,
                MinBedTemp = 90,
                MaxBedTemp = 110,
                Cooling = "Low (0-20%)",
                UL94Rating = "HB",
                RoHSCompliant = true,
                MaxWeightSupportKg = 75,
                Notes = "UV-resistant, similar to ABS. Use in enclosed printer. HB rating; suitable for outdoor electrical components. Supports ~75 kg; good for durable outdoor parts. RoHS compliant."
            },
            FilamentType.PC => new FilamentInfo
            {
                Type = FilamentType.PC,
                MinExtruderTemp = 250,
                MaxExtruderTemp = 280,
                MinBedTemp = 90,
                MaxBedTemp = 120,
                Cooling = "Low (0-20%)",
                UL94Rating = "V-0",
                RoHSCompliant = true,
                MaxWeightSupportKg = 100,
                Notes = "High strength, heat-resistant. Requires high-temp setup. V-0 rating (high flame resistance); ideal for critical electrical enclosures. Supports ~100 kg; best for heavy-duty electrical components. RoHS compliant."
            },
            FilamentType.HIPS => new FilamentInfo
            {
                Type = FilamentType.HIPS,
                MinExtruderTemp = 220,
                MaxExtruderTemp = 240,
                MinBedTemp = 80,
                MaxBedTemp = 100,
                Cooling = "Low (0-30%)",
                UL94Rating = "HB",
                RoHSCompliant = true,
                MaxWeightSupportKg = 60,
                Notes = "Soluble support material for ABS. Similar printing properties. HB rating; limited electrical use due to solubility. Supports ~60 kg; suitable for temporary structural supports. RoHS compliant."
            },
            FilamentType.PVA => new FilamentInfo
            {
                Type = FilamentType.PVA,
                MinExtruderTemp = 180,
                MaxExtruderTemp = 210,
                MinBedTemp = 45,
                MaxBedTemp = 60,
                Cooling = "Medium (30-50%)",
                UL94Rating = "Not Rated",
                RoHSCompliant = true,
                MaxWeightSupportKg = 20,
                Notes = "Water-soluble support material. Store in dry conditions. Not rated for UL94; unsuitable for electrical components. Supports ~20 kg; not for structural use. RoHS compliant."
            },
            FilamentType.CarbonFiber => new FilamentInfo
            {
                Type = FilamentType.CarbonFiber,
                MinExtruderTemp = 240,
                MaxExtruderTemp = 260,
                MinBedTemp = 60,
                MaxBedTemp = 80,
                Cooling = "Medium (30-50%)",
                UL94Rating = "HB",
                RoHSCompliant = true,
                MaxWeightSupportKg = 90,
                Notes = "High strength, abrasive. Use hardened steel nozzle. HB rating; suitable for structural electrical parts. Supports ~90 kg; excellent for lightweight, high-strength components. RoHS compliant."
            },
            FilamentType.ABS_GF => new FilamentInfo
            {
                Type = FilamentType.ABS_GF,
                MinExtruderTemp = 230,
                MaxExtruderTemp = 260,
                MinBedTemp = 80,
                MaxBedTemp = 100,
                Cooling = "Low (0-30%)",
                UL94Rating = "HB",
                RoHSCompliant = true,
                MaxWeightSupportKg = 80,
                Notes = "Glass fiber-reinforced ABS, high stiffness. Use hardened steel nozzle due to abrasion. HB rating; suitable for robust electrical housings. Supports ~80 kg; good for rigid, load-bearing parts. RoHS compliant."
            },
            FilamentType.PLA_FR => new FilamentInfo
            {
                Type = FilamentType.PLA_FR,
                MinExtruderTemp = 190,
                MaxExtruderTemp = 220,
                MinBedTemp = 20,
                MaxBedTemp = 60,
                Cooling = "High (50-100%)",
                UL94Rating = "V-0",
                RoHSCompliant = true,
                MaxWeightSupportKg = 55,
                Notes = "Flame-retardant PLA, easy to print. Use enclosed printer for best results. V-0 rating (high flame resistance); ideal for electrical safety components. Supports ~55 kg; good for lightweight, fire-safe parts. RoHS compliant."
            },
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown filament type: {type}")
        };
    }

    // Example view model for Avalonia UI
    public class FilamentViewModel
    {
        public IEnumerable<FilamentInfo> Filaments { get; private set; }

        public FilamentViewModel()
        {
            Filaments = Enum.GetValues<FilamentType>().Select(type => type.GetInfo());
        }

        // For async binding in UI, if needed
        public Task LoadFilamentsAsync()
        {
            Filaments = Enum.GetValues<FilamentType>().Select(type => type.GetInfo());
            return Task.CompletedTask;
        }
    }
}
