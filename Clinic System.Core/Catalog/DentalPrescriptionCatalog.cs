namespace Clinic_System.Core.Catalog;

public sealed record PrescriptionTemplateLine(
    string MedicationName,
    string Dosage,
    string Frequency,
    int DurationDays,
    string? SpecialInstructions);

public sealed record PrescriptionTemplate(
    string Key,
    string Name,
    string Category,
    string Indication,
    IReadOnlyList<PrescriptionTemplateLine> Lines);

public static class DentalPrescriptionCatalog
{
    public static IReadOnlyList<PrescriptionTemplate> All { get; } =
    [
        new("infeccion-amoxicilina", "Infección odontogénica", "Antibiótico",
            "Absceso, pericoronaritis o infección dental en adultos sin alergia a penicilina.",
            [
                new("Amoxicilina", "500 mg", "Cada 8 horas", 7, "Tomar después de las comidas. Completar el esquema aunque ceda el dolor.")
            ]),
        new("infeccion-amoxi-clav", "Infección odontogénica ampliada", "Antibiótico",
            "Infección persistente o con mayor cobertura. Valorar alergias y función hepática.",
            [
                new("Amoxicilina / ácido clavulánico", "875/125 mg", "Cada 12 horas", 7, "Tomar con alimentos. Suspender si aparece diarrea intensa o erupción.")
            ]),
        new("infeccion-clindamicina", "Infección — alergia a penicilina", "Antibiótico",
            "Alternativa en pacientes alérgicos a penicilina o amoxicilina.",
            [
                new("Clindamicina", "300 mg", "Cada 8 horas", 7, "Tomar con un vaso de agua. Avisar si hay diarrea persistente.")
            ]),
        new("absceso-mixto", "Absceso con componente anaerobio", "Antibiótico",
            "Infección mixta o absceso con fetidez. No usar metronidazol en embarazo sin criterio médico.",
            [
                new("Amoxicilina", "500 mg", "Cada 8 horas", 7, "Después de las comidas."),
                new("Metronidazol", "500 mg", "Cada 8 horas", 7, "Evitar alcohol durante el tratamiento y 48 horas después.")
            ]),
        new("analgesia-leve", "Analgesia leve", "Analgésico",
            "Dolor dental o postoperatorio leve. Preferible si hay gastritis o anticoagulación.",
            [
                new("Paracetamol", "500 mg", "Cada 6 a 8 horas si hay dolor", 5, "No exceder 4 g al día. Evitar con hepatopatía o alcoholismo.")
            ]),
        new("analgesia-aines", "Analgesia e inflamación", "Analgésico",
            "Dolor e inflamación odontogénica o postoperatoria. Precaución en úlcera, asma o insuficiencia renal.",
            [
                new("Ibuprofeno", "400 mg", "Cada 8 horas si hay dolor", 5, "Tomar después de las comidas. No combinar con otros AINE.")
            ]),
        new("dolor-postqx", "Dolor postoperatorio", "Analgésico",
            "Exodoncia, cirugía o endodoncia con dolor moderado.",
            [
                new("Ibuprofeno", "600 mg", "Cada 8 horas", 3, "Después de las comidas."),
                new("Paracetamol", "500 mg", "Cada 8 horas si persiste el dolor", 3, "Puede intercalarse con el ibuprofeno. No duplicar dosis.")
            ]),
        new("enjuague-clorhexidina", "Higiene postoperatoria / periodontal", "Antiséptico",
            "Tras cirugía, raspado o gingivitis. Evitar enjuagues vigorosos las primeras 24 h de una extracción.",
            [
                new("Gluconato de clorhexidina 0.12%", "15 ml", "Enjuague 2 veces al día (30–60 s)", 10, "No tragar. Esperar 30 minutos para cepillar o comer. Puede teñir dientes de forma reversible.")
            ]),
        new("post-exodoncia", "Paquete post-exodoncia", "Postoperatorio",
            "Extracción dental no complicada en adulto no alérgico a penicilina.",
            [
                new("Amoxicilina", "500 mg", "Cada 8 horas", 7, "Solo si el clínico indica cobertura antibiótica."),
                new("Ibuprofeno", "400 mg", "Cada 8 horas si hay dolor", 3, "Después de las comidas."),
                new("Gluconato de clorhexidina 0.12%", "15 ml", "Enjuague 2 veces al día desde las 24 h", 7, "No enjuagar con fuerza el día de la extracción.")
            ]),
        new("candidiasis-oral", "Candidiasis oral", "Antifúngico",
            "Estomatitis por Candida, prótesis o uso reciente de antibióticos.",
            [
                new("Nistatina suspensión oral", "5 ml (100 000 UI/ml)", "Enjuagar 4 veces al día y retener 2 minutos", 10, "No tragar de inmediato. Mantener la prótesis limpia.")
            ]),
        new("herpes-labial", "Herpes labial", "Antiviral",
            "Pródromo o lesiones labiales recurrentes. Iniciar lo antes posible.",
            [
                new("Aciclovir", "400 mg", "Cada 8 horas", 5, "Tomar con agua. No sustituye el control de factores desencadenantes.")
            ]),
        new("fluor-sensibilidad", "Sensibilidad dentinaria", "Preventivo",
            "Hipersensibilidad al frío o al cepillado. Complementa el tratamiento en consulta.",
            [
                new("Pasta dental con nitrato de potasio o flúor 1450 ppm", "Cinta de pasta", "Cepillado 2 veces al día", 14, "No enjuagar con abundante agua después del cepillado nocturno.")
            ])
    ];

    public static PrescriptionTemplate? Find(string key) =>
        All.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
}
