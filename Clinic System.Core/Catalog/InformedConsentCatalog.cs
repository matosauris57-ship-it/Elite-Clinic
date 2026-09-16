using Clinic_System.Core.Enums;

namespace Clinic_System.Core.Catalog;

public sealed record InformedConsentTemplate(
    InformedConsentType Type,
    string Title,
    string Purpose,
    string BodyText);

public static class InformedConsentCatalog
{
    public static readonly IReadOnlyList<InformedConsentTemplate> Templates =
    [
        new(
            InformedConsentType.EvaluationDiagnosis,
            "Consentimiento para evaluación y diagnóstico",
            "Autoriza la evaluación odontológica y los procedimientos diagnósticos necesarios.",
            """
            Se realizará una evaluación clínica odontológica que puede incluir historia clínica, exploración intraoral y extraoral, valoración de tejidos blandos y duros, oclusión y, si el odontólogo lo considera necesario, pruebas diagnósticas complementarias para establecer un diagnóstico y proponer un plan de tratamiento.

            El objetivo es identificar el estado de salud bucal, detectar enfermedades o alteraciones tempranas y orientar un tratamiento adecuado a las necesidades del paciente. Pueden presentarse molestias leves durante la exploración, sensibilidad temporal, o que el diagnóstico inicial requiera estudios adicionales.

            Como alternativas existen no realizar la evaluación (con el riesgo de no detectar problemas a tiempo) o solicitar una segunda opinión profesional. Declaro que he recibido información clara sobre la evaluación y los procedimientos diagnósticos, he podido formular preguntas y autorizo su realización. He tenido oportunidad de formular preguntas y recibir aclaraciones sobre el procedimiento indicado.
            """),

        new(
            InformedConsentType.Radiographs,
            "Consentimiento para radiografías",
            "Autoriza la realización de estudios radiográficos odontológicos.",
            """
            Se realizarán estudios radiográficos (por ejemplo periapicales, aleta de mordida, panorámica u otros indicados) con el fin de complementar el diagnóstico y planificar el tratamiento. Se utilizará la menor exposición razonable compatible con la necesidad clínica.

            El beneficio esperado es obtener información diagnóstica que no es visible a simple vista, planificar con mayor precisión y reducir riesgos durante el tratamiento. Entre los riesgos se incluyen la exposición a radiación ionizante (usualmente baja en odontología), la posible necesidad de repetir la toma si la imagen no es diagnóstica y molestias menores al posicionar el dispositivo.

            Como alternativas pueden limitarse o no realizarse radiografías (con menor certeza diagnóstica), diferir el estudio o valorar otras técnicas cuando estén clínicamente indicadas. Declaro que he sido informado(a) sobre la indicación, beneficios y riesgos de los estudios radiográficos y autorizo su realización. He tenido oportunidad de formular preguntas y recibir aclaraciones sobre el procedimiento indicado.
            """),

        new(
            InformedConsentType.LocalAnesthesia,
            "Consentimiento para anestesia local",
            "Deja constancia de la autorización para utilizar anestesia local.",
            """
            Se administrará anestesia local odontológica para controlar el dolor durante el procedimiento. Se informará el tipo de anestésico y se tomarán las precauciones habituales según los antecedentes del paciente.

            El beneficio esperado es reducir o eliminar el dolor durante el tratamiento y facilitar la realización segura del procedimiento. Entre los riesgos posibles figuran hematoma, parestesia temporal, taquicardia o nerviosismo transitorio, reacción alérgica (rara), toxicidad por sobredosis (rara), mordedura accidental de labio o mejilla mientras persista el efecto, e infección en el sitio de inyección (poco frecuente).

            Como alternativas existen realizar el procedimiento sin anestesia (con mayor dolor), usar otras técnicas de control del dolor cuando estén disponibles, o diferir el tratamiento. Declaro que he informado mis alergias y antecedentes relevantes, he comprendido los riesgos de la anestesia local y autorizo su uso. He tenido oportunidad de formular preguntas y recibir aclaraciones sobre el procedimiento indicado.
            """),

        new(
            InformedConsentType.Extraction,
            "Consentimiento para extracción dental",
            "Autoriza la extracción de una o más piezas dentales.",
            """
            Se procederá a la extracción de la(s) pieza(s) dental(es) indicada(s), previa evaluación clínica y, cuando corresponda, radiográfica. El procedimiento puede incluir anestesia local, remoción del diente y colocación de suturas si es necesario, junto con indicaciones de cuidado posterior.

            El beneficio esperado es eliminar una pieza no recuperable o indicada para extracción, aliviar dolor o infección y prevenir complicaciones mayores. Entre los riesgos se incluyen dolor, inflamación, sangrado, infección, alveolitis, daño a dientes o tejidos vecinos, comunicación oroantral, fractura de raíz, necesidad de cirugía adicional y parestesia temporal o, en casos raros, prolongada.

            Como alternativas existen tratamiento conservador (endodoncia, restauración, coronas), diferir la extracción o no tratar (con riesgo de empeoramiento). Declaro que he comprendido el procedimiento de extracción, sus beneficios, riesgos y alternativas, y autorizo su realización. He tenido oportunidad de formular preguntas y recibir aclaraciones sobre el procedimiento indicado.
            """),

        new(
            InformedConsentType.Endodontics,
            "Consentimiento para endodoncia",
            "Autoriza el tratamiento de conductos (endodoncia).",
            """
            Se realizará tratamiento de conductos en la pieza indicada: acceso a la cámara pulpar, limpieza y desinfección de conductos, obturación y, posteriormente, restauración definitiva según indicación clínica. Puede requerir más de una cita.

            El beneficio esperado es conservar la pieza dental, eliminar infección o inflamación pulpar o periapical y aliviar el dolor asociado. Entre los riesgos figuran dolor o sensibilidad postoperatoria, fractura del instrumento, perforación, fracaso del tratamiento que requiera retratamiento o extracción, decoloración y necesidad de corona u otra restauración para proteger el diente.

            Como alternativas existen la extracción de la pieza, no tratar (con riesgo de absceso o pérdida dental) o remisión a especialistas cuando esté indicado. Declaro que he sido informado(a) sobre la endodoncia, sus limitaciones y posibles complicaciones, y autorizo el tratamiento. He tenido oportunidad de formular preguntas y recibir aclaraciones sobre el procedimiento indicado.
            """),

        new(
            InformedConsentType.OralSurgery,
            "Consentimiento para cirugía oral",
            "Se utiliza para procedimientos quirúrgicos odontológicos.",
            """
            Se realizará un procedimiento de cirugía oral (por ejemplo, extracción quirúrgica, remoción de tejido, biopsia u otro acto quirúrgico odontológico indicado). Incluye anestesia local y cuidados postoperatorios que serán explicados por el odontólogo.

            El beneficio esperado es resolver la condición que motiva la cirugía, prevenir complicaciones y mejorar la función o salud bucal. Entre los riesgos se incluyen dolor, inflamación, hematoma, infección, sangrado, dehiscencia de suturas, daño a estructuras vecinas, parestesia, cicatrización lenta y, según el caso, necesidad de reintervención.

            Como alternativas existen tratamiento no quirúrgico cuando esté disponible, diferir la cirugía o remisión a cirugía maxilofacial según la complejidad. Declaro que he recibido explicación sobre la cirugía oral propuesta, he podido aclarar dudas y autorizo el procedimiento. He tenido oportunidad de formular preguntas y recibir aclaraciones sobre el procedimiento indicado.
            """),

        new(
            InformedConsentType.Implants,
            "Consentimiento para implantes dentales",
            "Autoriza la colocación de implantes dentales.",
            """
            Se planificará y realizará la colocación de uno o más implantes dentales (tornillos de titanio u otro material biocompatible) en el hueso maxilar o mandibular, con posible necesidad de injertos, membranas u otros procedimientos auxiliares. Posteriormente se colocará la prótesis sobre el implante según el plan acordado.

            El beneficio esperado es reponer dientes ausentes con una solución fija o removible sobre implantes, mejorar masticación, estética y calidad de vida. Entre los riesgos figuran infección, fracaso de oseointegración, pérdida ósea, daño a nervios o senos maxilares, necesidad de injertos adicionales, periimplantitis y que el resultado estético o funcional no coincida plenamente con las expectativas.

            Como alternativas existen prótesis removible, puente convencional, no reponer la pieza o diferir el tratamiento. Declaro que comprendo que el éxito del implante depende también de higiene, controles y factores individuales de cicatrización, y autorizo el tratamiento de implantes. He tenido oportunidad de formular preguntas y recibir aclaraciones sobre el procedimiento indicado.
            """),

        new(
            InformedConsentType.Orthodontics,
            "Consentimiento para ortodoncia",
            "Autoriza el inicio de un tratamiento de ortodoncia.",
            """
            Se iniciará un tratamiento ortodóntico con aparatos fijos, removibles o alineadores, según el plan indicado. Incluye controles periódicos, posibles ajustes y uso de retenedores al finalizar. La duración estimada puede variar según la respuesta biológica y la colaboración del paciente.

            El beneficio esperado es mejorar la alineación dental, oclusión, función masticatoria y, en muchos casos, la estética facial y dental. Entre los riesgos se incluyen molestias, ulceraciones, reabsorción radicular, caries o gingivitis si la higiene es insuficiente, recidiva si no se usan retenedores, necesidad de extracciones o cirugía auxiliar, y que los resultados dependan del cumplimiento del paciente.

            Como alternativas existen no tratar, un tratamiento limitado o cosmético, o remisión a ortodoncista especializado. Declaro que he sido informado(a) sobre el plan ortodóntico, duración aproximada, cuidados y riesgos, y autorizo el inicio del tratamiento. He tenido oportunidad de formular preguntas y recibir aclaraciones sobre el procedimiento indicado.
            """),

        new(
            InformedConsentType.Periodontal,
            "Consentimiento para tratamiento periodontal",
            "Autoriza procedimientos relacionados con las encías y el periodonto.",
            """
            Se realizarán procedimientos periodontales indicados (por ejemplo raspado y alisado radicular, control de placa, cirugía periodontal u otros tratamientos de encías) con el objetivo de controlar la enfermedad periodontal y preservar los dientes y tejidos de soporte.

            El beneficio esperado es reducir inflamación e infección gingival o periodontal, detener o ralentizar la pérdida de soporte y mejorar la salud de las encías. Entre los riesgos figuran sensibilidad dental, sangrado, molestias, recesión gingival aparente tras reducir la inflamación, movilidad temporal y que la enfermedad pueda recidivar sin mantenimiento y buena higiene.

            Como alternativas existen solo control de placa sin instrumentación, diferir el tratamiento o remisión a periodoncista. Declaro que comprendo la importancia del mantenimiento periodontal y autorizo el tratamiento indicado. He tenido oportunidad de formular preguntas y recibir aclaraciones sobre el procedimiento indicado.
            """),

        new(
            InformedConsentType.ClinicalImages,
            "Consentimiento para uso de imágenes clínicas",
            "Autoriza el uso de fotografías o imágenes del paciente para fines específicos.",
            """
            Se podrán tomar fotografías clínicas, radiografías u otras imágenes del paciente con fines de diagnóstico, seguimiento del tratamiento, documentación del expediente y, únicamente si se indica de forma expresa, fines educativos, académicos o de divulgación institucional.

            El beneficio esperado es documentar la evolución clínica, mejorar la comunicación del plan de tratamiento y, cuando se autorice, apoyar formación o divulgación profesional. Existe el riesgo de identificación visual del paciente si las imágenes se usan fuera del expediente; por ello, cualquier uso no clínico requerirá autorización específica y podrá limitarse o anonimizarse.

            Como alternativas existen autorizar solo el uso clínico en el expediente, negar el uso externo o solicitar anonimización. Autorizo la toma y el uso de imágenes clínicas en mi expediente. El uso fuera del ámbito clínico (docencia o divulgación) solo procederá si lo indico expresamente. He tenido oportunidad de formular preguntas y recibir aclaraciones sobre el procedimiento indicado.
            """),

        new(
            InformedConsentType.Other,
            "Consentimiento informado (otro procedimiento)",
            "Autoriza un procedimiento odontológico específico descrito por el profesional.",
            """
            Se realizará el procedimiento odontológico descrito por el odontólogo tratante, quien explicará en qué consiste, su justificación clínica y los cuidados posteriores. Los beneficios esperados, riesgos, complicaciones y alternativas disponibles serán los comunicados por el profesional según el caso clínico particular, incluyendo la opción de no tratarse.

            Declaro que he recibido información suficiente sobre el procedimiento indicado, he podido formular preguntas y autorizo su realización. He tenido oportunidad de formular preguntas y recibir aclaraciones sobre el procedimiento indicado.
            """)
    ];

    public static InformedConsentTemplate? Get(InformedConsentType type) =>
        Templates.FirstOrDefault(t => t.Type == type);

    public static InformedConsentTemplate GetOrDefault(InformedConsentType type) =>
        Get(type) ?? Templates.First(t => t.Type == InformedConsentType.Other);

    public static string NormalizeBody(string? body) =>
        string.IsNullOrWhiteSpace(body)
            ? string.Empty
            : body.Replace("\r\n", "\n").Trim();
}
