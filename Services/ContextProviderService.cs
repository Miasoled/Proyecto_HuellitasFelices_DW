using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;

namespace HuellitasFelices.Services;

public class ContextProviderService : IContextProviderService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ContextProviderService> _logger;

    private static readonly Dictionary<string[], string> KeywordQueries = new()
    {
        { new[] { "cliente", "clientes", "due\u00f1o", "due\u00f1os", "propietario" }, "clientes" },
        { new[] { "mascota", "mascotas", "perro", "perros", "gato", "gatos", "animal", "animales" }, "mascotas" },
        { new[] { "consulta", "consultas", "cita", "citas", "atencion" }, "consultas" },
        { new[] { "pendiente", "pendientes", "espera" }, "pendientes" },
        { new[] { "producto", "productos", "stock", "inventario", "existencia" }, "inventario" },
        { new[] { "venta", "ventas", "factura", "facturacion", "ingreso", "ingresos", "dinero", "cobro", "cobros", "vendido", "ultimas ventas", "ultimas", "reciente", "ultimamente", "vendio" }, "ventas" },
        { new[] { "doctor", "doctores", "veterinario", "veterinarios", "medico", "empleado", "empleados" }, "empleados" },
        { new[] { "adopcion", "adopciones", "solicitud", "solicitudes", "adoptar" }, "adopcion" },
        { new[] { "compra", "compras", "proveedor", "proveedores", "pedido" }, "compras" },
        { new[] { "tratamiento", "tratamientos", "medicamento", "medicamentos" }, "tratamientos" },
        { new[] { "resumen", "resumir", "general", "todo", "todo lo que", "dashboard", "estado" }, "resumen" }
    };

    private static readonly Dictionary<string[], string> ConocimientoGeneral = new()
    {
        {
            new[] { "diarrea", "diarreico", "diarreica", "heces blandas", "heces liquidas", "popo blanda", "caca blanda", "evacuaciones frecuentes" },
            "CUIDADO GENERAL - DIARREA EN MASCOTAS: " +
            "Causas: cambio de alimento, ingesta indigesta, infecciones virales/bacterianas, parasitosis, estr\u00e9s, alergias alimentarias, parvovirus. " +
            "Primeros pasos: Ayunar 12-24 horas (adultos), luego arroz blanco hervido sin condimentos con pollo desmenuzado. " +
            "Mantener hidrataci\u00f3n constante. Si hay sangre, v\u00f3mitos, moco o dura m\u00e1s de 48 horas: acudir al veterinario. " +
            "En cachorros la diarrea puede ser grave (parvovirus), buscar atenci\u00f3n urgente."
        },
        {
            new[] { "vomito", "vomitar", "v\u00f3mito", "regurgitar", "devolver", "lanza", "expulsa" },
            "CUIDADO GENERAL - VOMITO EN MASCOTAS: " +
            "Causas: comidas inapropiadas, sobrealimentaci\u00f3n, infecciones, par\u00e1sitos, cuerpos extra\u00f1os, gastritis, pancreatitis. " +
            "Primeros pasos: Retener alimento 12-24 horas, ofrecer agua peque\u00f1as cantidades. Despu\u00e9s, dieta blanda (arroz con pollo). " +
            "Si el v\u00f3mito contiene sangre, bilis, es continuo o dura m\u00e1s de 24 horas, acudir al veterinario."
        },
        {
            new[] { "vacuna", "vacunacion", "vacunar", "vacunas", "calendario", "refuerzo", "antirrabica", "polivalente" },
            "CUIDADO GENERAL - VACUNACION: " +
            "Perros: Primera vacuna a las 6-8 semanas, refuerzos cada 3-4 semanas hasta las 16 semanas. Refuerzo anual. " +
            "Vacunas b\u00e1sicas: moquillo, parvovirus, distemper, hepatitis, rabia. " +
            "Gatos: Primera a las 8-9 semanas, refuerzo a las 12 semanas. B\u00e1sicas: panleucopenia, calicivirus, herpesvirus, rabia."
        },
        {
            new[] { "desparasitar", "desparasitacion", "parasitos", "gusanos", "lombrices", "pulgas", "garrapatas", "desparasitante", "tenia", "anquilostoma" },
            "CUIDADO GENERAL - DESPARASITACION: " +
            "Internos: cada 3 meses en adultos, cada 2 semanas en cachorros hasta los 3 meses. " +
            "Externos: antipulgas y antigarrapatas seg\u00fan el producto indicado por el veterinario. " +
            "Se\u00f1ales: abdomen hinchado, diarrea con moco/sangre, pelo opaco, prurito anal, huevos blancos en heces."
        },
        {
            new[] { "alergia", "alergico", "alergica", "picaz\u00f3n", "rascarse", "ronchas", "urticaria", "dermatitis al\u00e9rgica" },
            "CUIDADO GENERAL - ALERGIAS: " +
            "Causas: alimentos, pulgas, ambientales (polvo, polen), qu\u00edmicos. " +
            "Se\u00f1ales: rascado excesivo, enrojecimiento de piel, p\u00e9rdida de pelo, inflamaci\u00f3n de o\u00eddos, hinchaz\u00f3n de hocico. " +
            "Ba\u00f1os con jab\u00f3n hipoalerg\u00e9nico pueden aliviar. El veterinario puede recetar antihistam\u00ednicos."
        },
        {
            new[] { "esterilizar", "esterilizacion", "castracion", "castrar", "covar", "ovariohisterectomia" },
            "CUIDADO GENERAL - ESTERILIZACION: " +
            "Recomendada a partir de los 6 meses. Beneficios: previene tumores mamarios, piometra, reduce comportamientos no deseados. " +
            "Procedimiento quir\u00fargico con anestesia general. Cuidados postoperatorios: reposo 7-10 d\u00edas, cono Elizabethano."
        },
        {
            new[] { "diente", "dientes", "dental", "limpieza dental", "sangrado de encias", "mal aliento", "halitosis", "sarro", "gingivitis" },
            "CUIDADO GENERAL - SALUD DENTAL: " +
            "Limpiar semanalmente con cepillo y pasta dental veterinaria. " +
            "Se\u00f1ales: mal aliento, sangrado de encias, dificultad para comer, sarro, p\u00e9rdida de dientes. " +
            "Limpieza profesional con anestesia. Prevenci\u00f3n: juguetes dentales, cepillado regular."
        },
        {
            new[] { "ojos", "ojo", "ocular", "lagrimeo", "secreccion ocular", "conjuntivitis", "ceguera", "opacidad", "pupila" },
            "CUIDADO GENERAL - SALUD OCULAR: " +
            "Se\u00f1ales de alarma: secreci\u00f3n abundante, enrojecimiento, hinchaz\u00f3n, opacidad, rascado constante. " +
            "Limpiar con soluci\u00f3n fisiol\u00f3gica o l\u00e1grimas artificiales veterinarias con gasa est\u00e9ril. No usar remedios caseros."
        },
        {
            new[] { "oreja", "orejas", "auditivo", "otitis", "cera", "mal olor oreja", "sacude la cabeza", "secrecion oscura" },
            "CUIDADO GENERAL - SALUD AUDITIVA: " +
            "Limpiar semanalmente con limpiador otol\u00f3gico veterinario y gasa est\u00e9ril. Nunca insertar hisopos. " +
            "Se\u00f1ales de infecci\u00f3n: mal olor, secreci\u00f3n oscura, sacudida de cabeza. Razas con orejas ca\u00eddas son m\u00e1s propensas."
        },
        {
            new[] { "herida", "heridas", "cortada", "cortado", "sangre", "sangrado", "golpe", "raspadura", "desgarro" },
            "CUIDADO GENERAL - HERIDAS: " +
            "Limpiar con soluci\u00f3n fisiol\u00f3gica o agua limpia. Presi\u00f3n con gasa est\u00e9ril si sangra. No usar alcohol o per\u00f3xido. " +
            "Cubrir con vendaje limpio. Si es profunda, sangra mucho o est\u00e1 infectada (pus, hinchaz\u00f3n), acudir al veterinario."
        },
        {
            new[] { "peso", "obesidad", "gordo", "adelgazar", "dieta", "alimentacion", "comida", "flaco", "bajo peso" },
            "CUIDADO GENERAL - ALIMENTACION Y PESO: " +
            "Croquetas de calidad seg\u00fan la edad. Porciones: 2-3% del peso corporal al d\u00eda. " +
            "Evitar: chocolate, uvas, cebolla, ajo, huesos cocidos, leche, xilitol. " +
            "Ejercicio diario: al menos 30 min de caminata. La obesidad causa diabetes, problemas articulares y card\u00edacos."
        },
        {
            new[] { "cachorro", "cachorros", "cachorra", "bebe perro", "bebe gato" },
            "CUIDADO GENERAL - CUIDADO DE CACHORROS: " +
            "Croquetas especiales 3-4 veces al d\u00eda hasta los 6 meses. Vacunaci\u00f3n a las 6-8 semanas. " +
            "Desparasitaci\u00f3n cada 2 semanas hasta los 3 meses. Socializaci\u00f3n entre 3-14 semanas. " +
            "No ba\u00f1ar hasta completar el esquema de vacunaci\u00f3n."
        },
        {
            new[] { "embarazo", "pre\u00f1ada", "gestacion", "parto", "cuidado embarazo", "cruza", "celo" },
            "CUIDADO GENERAL - EMBARAZO: " +
            "Duraci\u00f3n: perros 58-68 d\u00edas, gatos 63-67 d\u00edas. Se\u00f1ales: aumento de peso, hinchaz\u00f3n de pez\u00f3n. " +
            "Alimentaci\u00f3n de alta calidad, ejercicio moderado. Eco para contar cr\u00edas. " +
            "Acudir al veterinario si hay sangrado, fiebre o dificultad para parir."
        },
        {
            new[] { "fiebre", "temperatura", "calentura", "escalofrios", "temblor", "caliente", "arder" },
            "CUIDADO GENERAL - FIEBRE: " +
            "La temperatura normal varía según especie y tamaño. Señales de fiebre: letargo, p\u00e9rdida de apetito, temblores, nariz seca, ojos enrojecidos. " +
            "Medir con term\u00f3metro rectal si se tiene experiencia. Compresas fr\u00edas en patas. NUNCA dar ibuprofeno o paracetamol (son tóxicos para mascotas). " +
            "Si la fiebre es alta o dura más de 24 horas, acudir al veterinario."
        },
        {
            new[] { "tos", "toser", "estornudo", "gripe", "resfriado", "resfriada", "catarro" },
            "CUIDADO GENERAL - PROBLEMAS RESPIRATORIOS LEVES: " +
            "Causas: infecciones virales/bacterianas, alergias, bronquitis. " +
            "Mantener ambiente c\u00e1lido sin corrientes. Hidrataci\u00f3n constante. Vapor de agua puede ayudar. " +
            "Si hay dificultad para respirar, flema con sangre o dura m\u00e1s de 3 d\u00edas, acudir al veterinario urgente."
        },
        {
            new[] { "cojera", "cojo", "coja", "camina mal", "no camina", "articulacion", "artritis", "se arrastra", "paralisis" },
            "CUIDADO GENERAL - PROBLEMAS ARTICULARES: " +
            "Causas: traumatismos, displasia de cadera, artritis, lesiones de ligamentos, fracturas. " +
            "Reposo, no forzar movilidad. Compresas fr\u00edas las primeras 48 horas, despu\u00e9s calor. " +
            "Si no mejora en 24-48 horas o hay hinchaz\u00f3n, acudir al veterinario."
        },
        {
            new[] { "comez\u00f3n", "picar", "picaz\u00f3n", "rasgu\u00f1o", "se rasca mucho", "prurito", "se muerde la cola", "se lame mucho" },
            "CUIDADO GENERAL - PRURITO: " +
            "Causas: pulgas, alergias, hongos, piel seca, par\u00e1sitos externos. " +
            "Revisar pelaje en busca de pulgas o huevos (puntitos negros). Ba\u00f1os con avena coloidal pueden aliviar. " +
            "Consultar al veterinario para diagn\u00f3stico y tratamiento."
        },
        {
            new[] { "transporte", "viaje", "auto", "coche", "mareo", "marea", "estres viaje" },
            "CUIDADO GENERAL - TRANSPORTE Y VIAJE: " +
            "Mareo: no alimentar 2-3 horas antes. Ventilar el veh\u00edculo, paradas cada 2 horas. " +
            "Transportadora segura. Llevar agua, comida, documentaci\u00f3n veterinaria, vacunas al d\u00eda."
        },
        {
            new[] { "estre\u00f1imiento", "estre\u00f1ido", "constipado", "constipacion", "no caga", "heces duras", "heces secas" },
            "CUIDADO GENERAL - ESTRE\u00d1IMIENTO: " +
            "Causas: dieta baja en fibra, deshidrataci\u00f3n, falta de ejercicio, cuerpos extra\u00f1os. " +
            "Ofrecer mucha agua, a\u00f1adir calabaza enlatada a la comida, ejercicio ligero. " +
            "Si no defeca en 48 horas o hay v\u00f3mitos, acudir al veterinario. No usar laxantes humanos."
        },
        {
            new[] { "parvovirus", "parvovirosis", "parvo", "vomito con sangre", "diarrea con sangre", "sangre en heces" },
            "CUIDADO GENERAL - PARVOVIROSIS (EMERGENCIA): " +
            "Enfermedad viral muy grave en cachorros no vacunados. " +
            "S\u00edntomas: v\u00f3mito severo, diarrea hemorr\u00e1gica, letargo severo, p\u00e9rdida total de apetito. " +
            "ACUDIR AL VETERINARIO DE INMEDIATO. Requiere hospitalizaci\u00f3n con fluidoterapia IV. Alta mortalidad sin tratamiento."
        },
        {
            new[] { "moquillo", "distemper", "secrecion nasal", "secrecion ocular espesa", "costra nariz" },
            "CUIDADO GENERAL - MOQUILLO: " +
            "Enfermedad viral grave (respiratorio, digestivo, nervioso). S\u00edntomas: fiebre, secreci\u00f3n nasal/ocular espesa, tos. " +
            "Fase avanzada: convulsiones, temblores, hiperqueratosis. ACUDIR AL VETERINARIO URGENTE. Prevenci\u00f3n: vacunaci\u00f3n."
        },
        {
            new[] { "ingestion", "comio cosa", "trago", "trag\u00f3", "cuerpo extra\u00f1o", "objeto", "se comi\u00f3" },
            "CUIDADO GENERAL - INGESTI\u00d3N DE CUERPOS EXTRA\u00d1OS: " +
            "No inducir v\u00f3mito sin indicaci\u00f3n profesional. Identificar qu\u00e9 se ingiri\u00f3, cantidad y tiempo. " +
            "ACUDIR AL VETERINARIO. Si fue t\u00f3xico (chocolate, xilitol, raticida) es EMERGENCIA. No dar leche ni aceite."
        },
        {
            new[] { "intoxicacion", "envenenado", "raticida", "veneno", "pesticida", "chocolate toxico", "xilitol", "comida toxica" },
            "CUIDADO GENERAL - INTOXICACI\u00d3N (EMERGENCIA): " +
            "T\u00f3xicos comunes: raticida, chocolate, xilitol, uvas, cebolla, lirios (gatos), productos de limpieza. " +
            "ACUDIR AL VETERINARIO INMEDIATO. Llevar el envase. No inducir v\u00f3mito sin indicaci\u00f3n. No dar leche ni remedios caseros. Tiempo cr\u00edtico."
        },
        {
            new[] { "golpe de calor", "hipertermia", "jadeo excesivo", "lengua afuera", "se desmaya", "respira rapido" },
            "CUIDADO GENERAL - GOLPE DE CALOR (EMERGENCIA): " +
            "S\u00edntomas: jadeo excesivo, saliva espesa, encias rojas/azuladas, debilidad, v\u00f3mitos, colapso. " +
            "Enfriar gradualmente con agua fresca (NO helada) en cuello, axilas e ingles. ACUDIR AL VETERINARIO. " +
            "Nunca dejar mascotas en auto estacionado."
        },
        {
            new[] { "convulsion", "convulsiona", "ataque", "epilepsia", "se sacude", "temblores incontrolables", "cuerpo rigido", "crisis convulsiva" },
            "CUIDADO GENERAL - CONVULSIONES: " +
            "Causas: epilepsia, intoxicaci\u00f3n, hipoglucemia, enfermedades neurol\u00f3gicas. " +
            "Durante la convulsi\u00f3n: NO sostener ni meter dedos en la boca. Retirar objetos peligrosos. Anotar duraci\u00f3n. " +
            "Si dura m\u00e1s de 5 minutos o hay varias seguidas, es EMERGENCIA."
        },
        {
            new[] { "perdida de apetito", "no come", "no quiere comer", "anorexia", "rechaza la comida", "dejo de comer", "sin apetito" },
            "CUIDADO GENERAL - P\u00c9RDIDA DE APETITO: " +
            "Causas: estr\u00e9s, dolor, infecciones, problemas dentales, enfermedades internas. " +
            "Ofrecer alimento de olor fuerte (at\u00fan en agua, pollo hervido). Calentar la comida. " +
            "Si no come en 24 horas (adulto) o 12 horas (cachorro), acudir al veterinario."
        },
        {
            new[] { "letargo", "decaimiento", "cansancio", "fatiga", "debilidad", "adormilado", "sin energia", "apatico" },
            "CUIDADO GENERAL - LETARGO: " +
            "S\u00edntoma inespec\u00edfico que puede indicar m\u00faltiples enfermedades. " +
            "Causas: infecciones, dolor, anemia, hipoglucemia, deshidrataci\u00f3n. " +
            "Si dura m\u00e1s de 24 horas o es severo, acudir al veterinario para evaluaci\u00f3n completa."
        },
        {
            new[] { "deshidratacion", "piel seca", "nariz seca", "ojos hundidos", "pellejo levantado", "encias secas" },
            "CUIDADO GENERAL - DESHIDRATACI\u00d3N: " +
            "Signos: piel que se levanta lentamente al pellizcarla, encias secas, ojos hundidos. " +
            "Ofrecer agua peque\u00f1as cantidades frecuentes. Si hay signos claros, ACUDIR AL VETERINARIO para fluidos IV."
        },
        {
            new[] { "hipoglucemia", "azucar baja", "se desmaya", "temblores debiles", "colapso" },
            "CUIDADO GENERAL - HIPOGLUCEMIA: " +
            "Nivel bajo de az\u00fcar en sangre. M\u00e1s com\u00fan en cachorros y razas peque\u00f1as. " +
            "S\u00edntomas: temblores, debilidad, convulsiones, colapso. Aplicar miel o glucosa en encias. " +
            "ACUDIR AL VETERINARIO INMEDIATO. Puede ser mortal."
        },
        {
            new[] { "shock", "colapso cardiaco", "encias palidas", "pulso debil", "respiracion superficial" },
            "CUIDADO GENERAL - SHOCK (EMERGENCIA M\u00c1XIMA): " +
            "Enc\u00edas p\u00e1lidas o blancas, respiraci\u00f3n r\u00e1pida y superficial, pulso d\u00e9bil, temperatura baja, colapso. " +
            "Mantener caliente, no dar nada por boca. ACUDIR AL VETERINARIO DE INMEDIATO. Cada minuto cuenta."
        },
        {
            new[] { "ahogamiento", "se ahoga", "atragantado", "obstruccion respiratoria", "no respira", "asfixia" },
            "CUIDADO GENERAL - AHOGAMIENTO/ASFIXIA (EMERGENCIA): " +
            "Retirar el objeto visible SOLO si se ve f\u00e1cilmente. Compresiones tor\u00e1cicas. " +
            "Combinar 5 compresiones con 2 respiraciones de rescate. ACUDIR AL VETERINARIO."
        },
        {
            new[] { "mordedura", "mordida", "picadura", "arano", "escorpion", "serpiente", "vibora" },
            "CUIDADO GENERAL - MORDEDURAS Y PICADURAS: " +
            "Mordeduras de animales: limpiar, compresa fr\u00eda, acudir al vet. Picaduras: lavar, hielo, observar. " +
            "Serpiente o escorpi\u00f3n: EMERGENCIA. No aplicar torniquete. Mantener miembro afectado abajo del coraz\u00f3n."
        },
        {
            new[] { "fractura", "hueso roto", "hueso quebrado", "extremidad deformada", "no mueve la pata" },
            "CUIDADO GENERAL - FRACTURAS: " +
            "Cojera severa, extremidad en posici\u00f3n anormal, hinchaz\u00f3n, dolor intenso. " +
            "Inmovilizar con tabla o cart\u00f3n r\u00edgido. NO intentar acomodar el hueso. ACUDIR AL VETERINARIO URGENTE. No dar analg\u00e9sicos humanos."
        },
        {
            new[] { "quemadura", "quemado", "agua caliente", "acido", "quimico", "solar" },
            "CUIDADO GENERAL - QUEMADURAS: " +
            "Enfriar con agua fresca (NO helada) 10-15 minutos. Cubrir con gasa est\u00e9ril. No usar pasta de dientes ni mantequilla. " +
            "Quemaduras extensas o qu\u00edmicas requieren atenci\u00f3n veterinaria urgente."
        },
        {
            new[] { "tos seca", "tos continua", "tos nocturna", "jadeo", "respiracion agitada", "disnea", "ortopnea" },
            "CUIDADO GENERAL - DIFICULTAD RESPIRATORIA (EMERGENCIA): " +
            "Respiraci\u00f3n con boca abierta, costillas visibles, enc\u00edas azuladas, cuello extendido. " +
            "Mantener tranquila, ambiente fresco. ACUDIR AL VETERINARIO DE INMEDIATO."
        },
        {
            new[] { "neumonia", "flemas", "flema", "mucosidad", "secrecion nasal verde", "secrecion nasal amarilla" },
            "CUIDADO GENERAL - NEUMON\u00cdA: " +
            "Infecci\u00f3n del pulm\u00f3n. Tos con flemas, fiebre alta, dificultad respiratoria, letargo. " +
            "ACUDIR AL VETERINARIO URGENTE. Requiere antibi\u00f3ticos espec\u00edficos y posible hospitalizaci\u00f3n."
        },
        {
            new[] { "asma", "asma felino", "broncoespasmo", "silbido", "sibilancia" },
            "CUIDADO GENERAL - ASMA FELINO: " +
            "Enfermedad respiratoria cr\u00f3nica en gatos. Tos seca epis\u00f3dica, dificultad respiratoria. " +
            "Evitar al\u00e9rgenos: humo, polvo, ambientadores. Requiere diagn\u00f3stico y tratamiento con broncodilatadores."
        },
        {
            new[] { "problemas urinarios", "orina con sangre", "hematuria", "orina oscura", "cristales en orina", "urolitiasis", "sangre en orina" },
            "CUIDADO GENERAL - PROBLEMAS URINARIOS: " +
            "Causas: infecci\u00f3n, c\u00e1lculos/cristales, cistitis. Orinar frecuente en peque\u00f1as cantidades, esfuerzo, sangre. " +
            "Asegurar agua fresca constante. Los bloqueos urinarios (no poder orinar) son EMERGENCIA mortal en machos."
        },
        {
            new[] { "infeccion urinaria", "cistitis", "orina dentro", "orina en casa", "goteo de orina" },
            "CUIDADO GENERAL - INFECCI\u00d3N URINARIA: " +
            "Ganas frecuentes, peque\u00f1as cantidades, sangre, lugares inadecuados. " +
            "Mantener muy hidratada. Requiere antibi\u00f3tico espec\u00edfico. No automedicar. La infecci\u00f3n no tratada puede ascender al ri\u00f1\u00f3n."
        },
        {
            new[] { "insuficiencia renal", "ri\u00f1on", "ri\u00f1ones", "renal", "orina mucho", "bebe mucha agua", "poliuria", "polidipsia" },
            "CUIDADO GENERAL - INSUFICIENCIA RENAL: " +
            "Com\u00fan en gatos mayores. Bebe y orina excesivamente, p\u00e9rdida de peso, v\u00f3mitos, mal aliento con olor a amon\u00edaco. " +
            "Dieta renal prescrita. Asegurar hidrataci\u00f3n. No tiene cura pero se puede controlar."
        },
        {
            new[] { "tos ferina", "kennel cough", "traqueitis", "bronquitis infecciosa", "tos contagiosa perros" },
            "CUIDADO GENERAL - TOS DE LAS PERRERAS: " +
            "Infecci\u00f3n respiratoria contagiosa (Bordetella). Tos seca fuerte y persistente. " +
            "Generalmente leve en adultos. Aislamiento de otros perros. Usar arn\u00e9s en vez de correa. ACUDIR AL VETERINARIO si hay fiebre o pus nasal."
        },
        {
            new[] { "se rasca la cabeza", "tic", "tic nervioso", "movimientos repetitivos", "se rasca mucho la oreja" },
            "CUIDADO GENERAL - COMPORTAMIENTO COMPULSIVO: " +
            "Rascado, lamido o mordisqueo repetitivo pueden ser conductas estereotipadas (estr\u00e9s/ansiedad). " +
            "Primero descartar causa m\u00e9dica (alergias, par\u00e1sitos, dolor). Enriquecimiento ambiental, ejercicio diario."
        },
        {
            new[] { "ansiedad", "destruye", "ladra mucho", "llora cuando se va", "ansiedad por separacion", "no se queda solo" },
            "CUIDADO GENERAL - ANSIEDAD POR SEPARACI\u00d3N: " +
            "Destruir muebles, ladrar/llorar al quedarse solo, orinar dentro. " +
            "Desensibilizar: salidas cortas y crecientes, ignorar al llegar e irse. Rutinas predecibles. Consultar al veterinario."
        },
        {
            new[] { "agresion", "agresivo", "muerde", "mordio", "ataca", "se enoja", "dominancia" },
            "CUIDADO GENERAL - AGRESI\u00d3N: " +
            "Causas: miedo, dolor, protecci\u00f3n de recurso, territorialidad. NUNCA castigar f\u00edsicamente. " +
            "La agresi\u00f3n repentina puede indicar enfermedad neurol\u00f3gica o dolor. Consultar al veterinario/et\u00f3logo."
        },
        {
            new[] { "miedo", "asustado", "ruido", "truenos", "fuegos artificiales", "pavor", "panico" },
            "CUIDADO GENERAL - MIEDO A RUIDOS: " +
            "Crear refugio seguro. No forzar ni consolar excesivamente. M\u00fAsica suave o ruido blanco. " +
            "Camisetas de compresi\u00f3n. Para casos severos, consultar al veterinario sobre ansiol\u00edticos."
        },
        {
            new[] { "se pone nervioso", "nervioso", "estres", "estresado", "inquieto", "hiperactivo" },
            "CUIDADO GENERAL - ESTR\u00c9S: " +
            "Se\u00f1ales: aseo excesivo, p\u00e9rdida de pelo por lamido, cambios de apetito, automutilaci\u00f3n. " +
            "Establecer rutinas regulares. Ejercicio diario. Feromonas sint\u00e9ticas (Adaptil/Feliway) pueden ayudar."
        },
        {
            new[] { "no duerme", "insomnio", "duerme mucho", "se despierta de noche", "ladra de noche", "llora de noche" },
            "CUIDADO GENERAL - ALTERACIONES DEL SUE\u00d1O: " +
            "Causas: ansiedad, dolor, problemas urinarios, vejez, s\u00edndrome cognitivo canino. " +
            "Rutina de sue\u00f1o regular. Ejercicio diario adecuado a la edad."
        },
        {
            new[] { "come pelo", "bolas de pelo", "pelo en vomito", "vomito con pelo", "tricobezoar", "trichobezoars" },
            "CUIDADO GENERAL - BOLAS DE PELO (GATOS): " +
            "Normal en gatos. Prevenci\u00f3n: cepillado frecuente, pastas de expulsi\u00f3n, alimento con m\u00e1s fibra. " +
            "Si vomita pelo frecuentemente o hay obstrucci\u00f3n (no come, v\u00f3mito continuo), acudir al veterinario."
        },
        {
            new[] { "se come la caca", "coprofagia", "come caca de otro", "come caca de gato", "come heces" },
            "CUIDADO GENERAL - COPROFAGIA: " +
            "Causas: deficiencias nutricionales, estr\u00e9s, aburrimiento. En cachorros puede ser normal hasta los 6 meses. " +
            "Recoger heces inmediatamente, no rega\u00f1ar. Enriquecimiento ambiental. Consultar al veterinario."
        },
        {
            new[] { "se lame las patas", "lamido excesivo", "pata hinchada", "pata roja", "granuloma por lamido" },
            "CUIDADO GENERAL - LAMIDO EXCESIVO: " +
            "Causas: alergias, dolor localizado, ansiedad, infecci\u00f3n. La saliva ti\u00f1e el pelo de marr\u00f3n. " +
            "Primero descartar causa m\u00e9dica. Usar collar Elizabethano para romper el ciclo."
        },
        {
            new[] { "perdida de pelo", "se le cae el pelo", "calvo", "aloppecia", "pelaje opaco", "se pela", "muda excesiva" },
            "CUIDADO GENERAL - P\u00c9RDIDA DE PELO: " +
            "Causas: alergias, par\u00e1sitos, hongos (ti\u00f1a), hormonas, estr\u00e9s, deficiencias nutricionales. " +
            "\u00c1reas redondas sin pelo: puede ser ti\u00f1a (contagiosa). P\u00e9rdida sim\u00e9trica: hormonal. ACUDIR AL VETERINARIO."
        },
        {
            new[] { "ti\u00f1a", "hongos", "hongo", "dermatofitosis", "anillo rojo", "placa calva" },
            "CUIDADO GENERAL - TI\u00d1A: Infeccion fungica contagiosa. Areas redondas sin pelo, costras, enrojecimiento. " +
            "Contagiosa para humanos. Aislar. Tratamiento: antifungicos segun prescripcion veterinaria."
        },
        {
            new[] { "sarna", "sarnico", "notoedres", "sarcoptes", "demodex", "demodicosis", "acaro" },
            "CUIDADO GENERAL - SARNAS: " +
            "Sarcoptica: picaz\u00f3n severa, costras (contagiosa a humanos). Demod\u00e9x: p\u00e9rdida de pelo alrededor de ojos y patas. " +
            "Tratamiento: antiparasitarios espec\u00edficos seg\u00fan prescripci\u00f3n. ACUDIR AL VETERINARIO."
        },
        {
            new[] { "golpes", "choque", "impacto", "traumatismo", "se cayo" },
            "CUIDADO GENERAL - GOLPES Y TRAUMATISMOS: " +
            "\u00bfEst\u00e1 consciente? \u00bfRespira bien? \u00bfPuede moverse? Compresa fr\u00eda en la zona. " +
            "Observar 24-48 horas: letargo, v\u00f3mitos pueden indicar da\u00f1o interno. Si fue severo, acudir al veterinario."
        },
        {
            new[] { "marcado urinario", "orina en sitio", "marca territorio", "orinar en objetos", "levanta la pata dentro" },
            "CUIDADO GENERAL - MARCADO URINARIO: " +
            "Normal en machos no castrados. La esterilizaci\u00f3n reduce el marcado. " +
            "Limpiar con limpiadores enzim\u00e1ticos. Consultar al veterinario si es severo."
        },
        {
            new[] { "ojos llorosos", "lagrimeo excesivo", "moco en el ojo", "ojo rojo", "ojo hinchado", "ojo cerrado", "ojo seco" },
            "CUIDADO GENERAL - PROBLEMAS OCULARES: " +
            "Lagrimeo claro: alergia/irritaci\u00f3n leve. Secreci\u00f3n verde/amarilla: infecci\u00f3n. " +
            "Ojo rojo e hinchado: \u00falcera, glaucoma (EMERGENCIA). No usar gotas humanas. ACUDIR AL VETERINARIO."
        },
        {
            new[] { "ulcera corneal", "opacidad corneal", "nube en el ojo", "mancha blanca en el ojo" },
            "CUIDADO GENERAL - \u00daLCERA CORNEAL: " +
            "Erosi\u00f3n en la c\u00f3rnea. Dolor intenso, ojo cerrado, lagrimeo, opacidad blanca/gris. " +
            "EMERGENCIA OCULAR. No tocar ni frotar. ACUDIR AL VETERINARIO INMEDIATO."
        },
        {
            new[] { "glaucoma", "presion alta ojo", "ojo muy hinchado", "ojo muy rojo" },
            "CUIDADO GENERAL - GLAUCOMA: " +
            "Aumento de presi\u00f3n intraocular. EMERGENCIA. Puede causar ceguera irreversible en 24-48 horas. " +
            "Ojo muy rojo e hinchado, pupila dilatada, c\u00f3rnea opaca. ACUDIR AL VETERINARIO INMEDIATO."
        },
        {
            new[] { "proptosis", "ojo salido", "ojo fuera", "ojo protruido" },
            "CUIDADO GENERAL - PROPTOSIS (EMERGENCIA M\u00c1XIMA): " +
            "El globo ocular se sale de la \u00f3rbita. Com\u00fan en razas braquic\u00e9falas. " +
            "NO reposicionar. Mantener h\u00fa medo. Cubrir con gasa h\u00fameda y transportar al veterinario INMEDIATAMENTE."
        },
        {
            new[] { "oreja hinchada", "oreja roja", "rasca oreja", "secrecion oscura oreja", "otitis externa" },
            "CUIDADO GENERAL - OTITIS: " +
            "Causas: \u00e1caros, bacterias, hongos, alergias. Sacudida de cabeza, secreci\u00f3n oscura, mal olor. " +
            "Limpiar parte visible con limpiador otol\u00f3gico. No meter hisopos. ACUDIR AL VETERINARIO. Sin tratamiento puede causar sordera."
        },
        {
            new[] { "piometra", "infeccion uterina", "secrecion vaginal", "pus vagina", "abdomen hinchado hembra" },
            "CUIDADO GENERAL - PIOMETRA (EMERGENCIA): " +
            "Infecci\u00f3n del \u00fatero con pus. 2-8 semanas despu\u00e9s del celo en hembras no esterilizadas. " +
            "Secreci\u00f3n vaginal purulenta, abdomen hinchado, letargo, fiebre. ACUDIR AL VETERINARIO URGENTE. Requiere cirug\u00eda."
        },
        {
            new[] { "celo", "calor", "hembras en celo", "sangrado vaginal hembra", "ahechaura" },
            "CUIDADO GENERAL - CELO: " +
            "Cada 6-8 meses, dura ~3 semanas. Sangrado vaginal, elevaci\u00f3n de cola, monta. " +
            "F\u00e9rtil d\u00eda 10-14. Evitar paseos sin correa. Si hay fiebre o flujo anormal, puede ser piometra (EMERGENCIA)."
        },
        {
            new[] { "tumor", "masa", "bulto", "crecimiento", "n\u00f3dulo" },
            "CUIDADO GENERAL - TUMORES/MASSAS: " +
            "Pueden ser benignos o malignos. Evaluar tama\u00f1o, consistencia, movilidad, velocidad de crecimiento. " +
            "ACUDIR AL VETERINARIO para biopsia o citolog\u00eda. Diagn\u00f3stico temprano mejora pron\u00f3stico."
        },
        {
            new[] { "diabetes", "diabetico", "azucar alta", "bebe mucha orina mucha", "come mucho adelgaza" },
            "CUIDADO GENERAL - DIABETES: " +
            "Com\u00fan en perros/gatos obesos o de edad avanzada. Bebe y orina excesivamente, come mucho pero pierde peso. " +
            "Requiere insulina diaria y dieta espec\u00edfica. Con manejo adecuado, vida normal."
        },
        {
            new[] { "hipotiroidismo", "tiroides", "tiroides baja", "pelo seco", "piel gruesa" },
            "CUIDADO GENERAL - HIPOTIROIDISMO (PERROS): " +
            "Ganancia de peso, pelo seco, piel gruesa, letargo, intolerancia al fr\u00edo. " +
            "Diagn\u00f3stico con an\u00e1lisis de sangre (T4). Tratamiento: levotiroxina oral diaria."
        },
        {
            new[] { "hipertiroidismo", "tiroides alta", "adelgaza come mucho", "vomita mucho gato" },
            "CUIDADO GENERAL - HIPERTIROIDISMO (GATOS): " +
            "Come mucho pero pierde peso, v\u00f3mitos, hiperactividad, cardiopat\u00eda. " +
            "Diagn\u00f3stico (T4). Tratamiento: metimazol, yodo radiactivo, cirug\u00eda."
        },
        {
            new[] { "cushing", "cortisol alto", "vientre hinchado", "bebe mucho", "pelo se cae simetrico" },
            "CUIDADO GENERAL - S\u00cdNDROME DE CUSHING: " +
            "Exceso de cortisol. Vientre distendido, p\u00e9rdida de pelo sim\u00e9trica, bebe y orina mucho. " +
            "Tratamiento: trilostano o cirug\u00eda. ACUDIR AL VETERINARIO."
        },
        {
            new[] { "addison", "cortisol bajo", "colapso", "debilidad episodica" },
            "CUIDADO GENERAL - ENFERMEDAD DE ADDISON: " +
            "Deficiencia de hormonas suprarrenales. Episodios de letargo, v\u00f3mitos, debilidad. " +
            "Crisis de Addison es emergencia. Tratamiento de por vida: fludrocortisona."
        },
        {
            new[] { "pancreatitis", "pancreas", "dolor abdominal", "vomito con bilis", "abdomen duro" },
            "CUIDADO GENERAL - PANCREATITIS: " +
            "Inflamaci\u00f3n del p\u00e1ncreas. V\u00f3mitos, dolor abdominal (postura de oraci\u00f3n), diarrea, letargo. " +
            "ACUDIR AL VETERINARIO URGENTE. Dieta baja en grasas despu\u00e9s del episodio."
        },
        {
            new[] { "torsion gastrica", "estomago retorcido", "bloat", "vientre hinchado perro grande", "no puede vomitar perro grande" },
            "CUIDADO GENERAL - TORSI\u00d3N G\u00c1STRICA (EMERGENCIA M\u00c1XIMA): " +
            "Com\u00fan en razas grandes/gigantes. Abdomen hinchado y duro, intentos de vomitar sin resultado, colapso. " +
            "ACUDIR AL VETERINARIO DE INMEDIATO. Mortal sin cirug\u00eda urgente. Prevenci\u00f3n: comidas peque\u00f1as, no ejercicio intenso post-comida."
        },
        {
            new[] { "barro", "come tierra", "pica", "comportamiento pica" },
            "CUIDADO GENERAL - PICA (COMER TIERRA): " +
            "Causas: deficiencias nutricionales, parasitosis, estr\u00e9s, n\u00e1useas. En cachorros puede ser exploraci\u00f3n normal. " +
            "Verificar desparasitaci\u00f3n y nutrici\u00f3n. ACUDIR AL VETERINARIO si es compulsivo."
        },
        {
            new[] { "comer piedras", "litofagia", "traga piedras" },
            "CUIDADO GENERAL - LITOFAGIA (COMER PIEDRAS): " +
            "Riesgo de obstrucci\u00f3n intestinal o perforaci\u00f3n. Puede indicar deficiencia mineral. " +
            "ACUDIR AL VETERINARIO URGENTE si fue reciente. Vigilar: v\u00f3mitos, dolor abdominal, no come, no defeca."
        },
        {
            new[] { "baba excesiva", "hipersalivacion", "babosea", "saliva excesiva", "se lame los labios" },
            "CUIDADO GENERAL - HIPERSALIVACI\u00d3N: " +
            "Causas: n\u00e1useas, dolor oral, cuerpos extra\u00f1os, infecci\u00f3n dental, intoxicaci\u00f3n, estr\u00e9s. " +
            "Revisar la boca: \u00bfHay algo atrapado? \u00bfSangrado? Si es episodio breve puede ser n\u00e1usea transitoria."
        },
        {
            new[] { "pancreatits", "pancreas inflamado" },
            "CUIDADO GENERAL - PANCREATITIS: " +
            "Inflamaci\u00f3n del p\u00e1ncreas. Dieta alta en grasas, obesidad. V\u00f3mitos, dolor abdominal, diarrea, letargo. " +
            "ACUDIR AL VETERINARIO URGENTE. Requiere hospitalizaci\u00f3n. Dieta baja en grasas posterior."
        },
        {
            new[] { "barro", "come tierra", "pica", "comportamiento pica", "comio tierra" },
            "CUIDADO GENERAL - PICA (COMER TIERRA): " +
            "Causas: deficiencias nutricionales, parasitosis, estr\u00e9s, n\u00e1useas. En cachorros puede ser normal. " +
            "Verificar desparasitaci\u00f3n. ACUDIR AL VETERINARIO si es compulsivo."
        }
    };

    public ContextProviderService(AppDbContext db, ILogger<ContextProviderService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<string> ObtenerContextoAsync(string preguntaUsuario)
    {
        var pregunta = preguntaUsuario.ToLowerInvariant();
        var contextos = new List<string>();

        var historialMascota = await ObtenerHistorialMascotaAsync(pregunta);
        if (!string.IsNullOrWhiteSpace(historialMascota))
            contextos.Add(historialMascota);

        var categoriasDetectadas = new HashSet<string>();

        foreach (var (keywords, categoria) in KeywordQueries)
        {
            if (keywords.Any(k => pregunta.Contains(k)))
            {
                categoriasDetectadas.Add(categoria);
            }
        }

        if (categoriasDetectadas.Count == 0 && contextos.Count == 0)
        {
            categoriasDetectadas.Add("resumen");
        }

        foreach (var cat in categoriasDetectadas)
        {
            try
            {
                var contexto = await ObtenerContextoPorCategoriaAsync(cat, pregunta);
                if (!string.IsNullOrWhiteSpace(contexto))
                    contextos.Add(contexto);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener contexto para categoría: {Categoria}", cat);
            }
        }

        if (contextos.Count > 0)
            return string.Join(" | ", contextos);

        foreach (var (keywords, consejo) in ConocimientoGeneral)
        {
            if (keywords.Any(k => pregunta.Contains(k)))
            {
                return consejo;
            }
        }

        return "No hay datos en la base de datos para esta pregunta. Puedes preguntar sobre el estado de la clinica (mascotas, consultas, ventas, inventario, etc.) o sobre cuidados generales de mascotas (diarrea, vacunas, alimentacion, desparasitacion, etc.).";
    }

    private async Task<string> ObtenerContextoPorCategoriaAsync(string categoria, string pregunta)
    {
        var hoy = DateTime.UtcNow.Date;

        return categoria switch
        {
            "clientes" => await ObtenerContextoClientesAsync(),
            "mascotas" => await ObtenerContextoMascotasAsync(),
            "consultas" => await ObtenerContextoConsultasAsync(pregunta, hoy),
            "pendientes" => await ObtenerContextoPendientesAsync(),
            "inventario" => await ObtenerContextoInventarioAsync(),
            "ventas" => await ObtenerContextoVentasAsync(pregunta, hoy),
            "empleados" => await ObtenerContextoEmpleadosAsync(),
            "adopcion" => await ObtenerContextoAdopcionAsync(),
            "compras" => await ObtenerContextoComprasAsync(),
            "tratamientos" => await ObtenerContextoTratamientosAsync(),
            "resumen" => await ObtenerContextoResumenAsync(hoy),
            _ => ""
        };
    }

    private async Task<string> ObtenerContextoClientesAsync()
    {
        var total = await _db.Duenos.CountAsync(d => d.Activo);
        var nuevosMes = await _db.Duenos.CountAsync(d => d.Activo && d.FechaCreacion.Month == DateTime.UtcNow.Month && d.FechaCreacion.Year == DateTime.UtcNow.Year);
        return $"Total de clientes activos: {total}. Nuevos este mes: {nuevosMes}.";
    }

    private async Task<string> ObtenerContextoMascotasAsync()
    {
        var total = await _db.Mascotas.CountAsync(m => m.Activo);
        var porEspecie = await _db.Mascotas
            .Where(m => m.Activo)
            .GroupBy(m => m.Especie)
            .Select(g => $"{g.Key}: {g.Count()}")
            .ToListAsync();
        var detalle = string.Join(", ", porEspecie);
        return $"Total de mascotas: {total}. Por especie: {detalle}.";
    }

    private async Task<string> ObtenerContextoConsultasAsync(string pregunta, DateTime hoy)
    {
        if (pregunta.Contains("hoy"))
        {
            var hoyCount = await _db.Consultas.CountAsync(c => c.Activo && c.FechaConsulta.Date == hoy);
            return $"Consultas hoy: {hoyCount}.";
        }
        if (pregunta.Contains("semana"))
        {
            var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);
            var semanaCount = await _db.Consultas.CountAsync(c => c.Activo && c.FechaConsulta.Date >= inicioSemana);
            return $"Consultas esta semana: {semanaCount}.";
        }
        if (pregunta.Contains("mes"))
        {
            var mesCount = await _db.Consultas.CountAsync(c => c.Activo && c.FechaConsulta.Month == hoy.Month && c.FechaConsulta.Year == hoy.Year);
            return $"Consultas este mes: {mesCount}.";
        }
        var total = await _db.Consultas.CountAsync(c => c.Activo);
        var completadas = await _db.Consultas.CountAsync(c => c.Activo && c.Estado == "Completada");
        var pendientes = await _db.Consultas.CountAsync(c => c.Activo && c.Estado == "Pendiente");
        var enRevision = await _db.Consultas.CountAsync(c => c.Activo && c.Estado == "EnRevision");
        return $"Total consultas: {total}. Completadas: {completadas}. En revision: {enRevision}. Pendientes: {pendientes}.";
    }

    private async Task<string> ObtenerContextoPendientesAsync()
    {
        var consultasPend = await _db.Consultas.CountAsync(c => c.Activo && c.Estado == "Pendiente");
        var consultasRev = await _db.Consultas.CountAsync(c => c.Activo && c.Estado == "EnRevision");
        var solicitudesPend = await _db.SolicitudesAdopcion.CountAsync(s => s.Activo && s.Estado == "Pendiente");
        var comprasPend = await _db.Compras.CountAsync(c => c.Activo && c.Estado == "Pendiente");
        return $"Pendientes - Consultas: {consultasPend}, En revision: {consultasRev}, Solicitudes adopcion: {solicitudesPend}, Compras a proveedores: {comprasPend}.";
    }

    private async Task<string> ObtenerContextoInventarioAsync()
    {
        var productos = await _db.Productos
            .Where(p => p.Activo)
            .Include(p => p.Inventarios)
            .Include(p => p.Categoria)
            .Select(p => new { p.Nombre, Stock = p.Inventarios.Any() ? p.Inventarios.First().StockActual : 0, p.StockMinimo, Categoria = p.Categoria!.Nombre })
            .ToListAsync();

        var total = productos.Count;
        var bajos = productos.Where(p => p.Stock <= p.StockMinimo).ToList();
        var sinStock = productos.Where(p => p.Stock == 0).ToList();

        var resultado = $"Total productos: {total}. Sin stock: {sinStock.Count}. Stock bajo el minimo: {bajos.Count}.";
        if (bajos.Any())
        {
            var listaBajos = string.Join(", ", bajos.Take(5).Select(p => $"{p.Nombre}({p.Stock}u)"));
            resultado += $" Productos criticos: {listaBajos}.";
        }
        return resultado;
    }

    private async Task<string> ObtenerContextoVentasAsync(string pregunta, DateTime hoy)
    {
        if (pregunta.Contains("hoy"))
        {
            var ventasHoy = await _db.Ventas
                .Where(v => v.Activo && v.Estado == "Pagada" && v.FechaVenta.Date == hoy)
                .CountAsync();
            var montoHoy = await _db.Ventas
                .Where(v => v.Activo && v.Estado == "Pagada" && v.FechaVenta.Date == hoy)
                .SumAsync(v => v.Total);
            return $"Ventas hoy: {ventasHoy}. Ingresos hoy: ${montoHoy:F2}.";
        }
        if (pregunta.Contains("mes"))
        {
            var ventasMes = await _db.Ventas
                .Where(v => v.Activo && v.Estado == "Pagada" && v.FechaVenta.Month == hoy.Month && v.FechaVenta.Year == hoy.Year)
                .CountAsync();
            var montoMes = await _db.Ventas
                .Where(v => v.Activo && v.Estado == "Pagada" && v.FechaVenta.Month == hoy.Month && v.FechaVenta.Year == hoy.Year)
                .SumAsync(v => v.Total);
            return $"Ventas este mes: {ventasMes}. Ingresos este mes: ${montoMes:F2}.";
        }
        var totalVentas = await _db.Ventas.CountAsync(v => v.Activo && v.Estado == "Pagada");
        var totalIngresos = await _db.Ventas.Where(v => v.Activo && v.Estado == "Pagada").SumAsync(v => v.Total);
        var pendientesPago = await _db.Ventas.CountAsync(v => v.Activo && v.Estado == "Pendiente");
        var resultado = $"Total ventas pagadas: {totalVentas}. Ingresos totales: ${totalIngresos:F2}. Ventas pendientes de pago: {pendientesPago}.";

        var ultimasVentas = await _db.Ventas
            .Where(v => v.Activo && v.Estado == "Pagada")
            .OrderByDescending(v => v.FechaVenta)
            .Take(5)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .ToListAsync();

        if (ultimasVentas.Any())
        {
            var detalle = string.Join(" | ", ultimasVentas.Select(v =>
            {
                var productos = v.Detalles.Any()
                    ? string.Join(", ", v.Detalles.Select(d => $"{d.Producto?.Nombre ?? "?"} x{d.Cantidad}"))
                    : "Servicio";
                return $"[{v.FechaVenta:dd/MM/yyyy}] ${v.Total:F2} - {productos}";
            }));
            resultado += $" Ultimas 5 ventas: {detalle}";
        }

        return resultado;
    }

    private async Task<string> ObtenerContextoEmpleadosAsync()
    {
        var total = await _db.Empleados.CountAsync(e => e.Activo);
        var porCargo = await _db.Empleados
            .Where(e => e.Activo)
            .GroupBy(e => e.Cargo)
            .Select(g => $"{g.Key}: {g.Count()}")
            .ToListAsync();
        var detalle = string.Join(", ", porCargo);
        return $"Total empleados activos: {total}. Por cargo: {detalle}.";
    }

    private async Task<string> ObtenerContextoAdopcionAsync()
    {
        var totalAnimales = await _db.AnimalesAdopcion.CountAsync(a => a.Activo && a.Disponible);
        var solicitudesPend = await _db.SolicitudesAdopcion.CountAsync(s => s.Activo && s.Estado == "Pendiente");
        var solicitudesTotal = await _db.SolicitudesAdopcion.CountAsync(s => s.Activo);
        return $"Animales disponibles para adopcion: {totalAnimales}. Solicitudes de adopcion pendientes: {solicitudesPend}. Total solicitudes: {solicitudesTotal}.";
    }

    private async Task<string> ObtenerContextoComprasAsync()
    {
        var pendientes = await _db.Compras.CountAsync(c => c.Activo && c.Estado == "Pendiente");
        var recibidas = await _db.Compras.CountAsync(c => c.Activo && c.Estado == "Recibida");
        var totalMes = await _db.Compras.CountAsync(c => c.Activo && c.FechaCompra.Month == DateTime.UtcNow.Month && c.FechaCompra.Year == DateTime.UtcNow.Year);
        return $"Compras pendientes: {pendientes}. Recibidas: {recibidas}. Compras este mes: {totalMes}.";
    }

    private async Task<string> ObtenerContextoTratamientosAsync()
    {
        var total = await _db.Tratamientos.CountAsync(t => t.Activo);
        var recientes = await _db.Tratamientos
            .Where(t => t.Activo)
            .OrderByDescending(t => t.FechaCreacion)
            .Take(3)
            .Select(t => t.Nombre)
            .ToListAsync();
        var detalle = recientes.Any() ? string.Join(", ", recientes) : "ninguno reciente";
        return $"Total tratamientos registrados: {total}. Ultimos: {detalle}.";
    }

    private async Task<string> ObtenerContextoResumenAsync(DateTime hoy)
    {
        var clientes = await _db.Duenos.CountAsync(d => d.Activo);
        var mascotas = await _db.Mascotas.CountAsync(m => m.Activo);
        var consultasHoy = await _db.Consultas.CountAsync(c => c.Activo && c.FechaConsulta.Date == hoy);
        var consultasPend = await _db.Consultas.CountAsync(c => c.Activo && c.Estado == "Pendiente");
        var ventasMes = await _db.Ventas.CountAsync(v => v.Activo && v.Estado == "Pagada" && v.FechaVenta.Month == hoy.Month && v.FechaVenta.Year == hoy.Year);
        var ingresosMes = await _db.Ventas.Where(v => v.Activo && v.Estado == "Pagada" && v.FechaVenta.Month == hoy.Month && v.FechaVenta.Year == hoy.Year).SumAsync(v => v.Total);
        var productos = await _db.Productos.CountAsync(p => p.Activo);
        var veterinarios = await _db.Empleados.CountAsync(e => e.Activo && e.Cargo == "Veterinario");
        var adopciones = await _db.AnimalesAdopcion.CountAsync(a => a.Activo && a.Disponible);

        return $"Resumen clinica Huellitas Felices: {clientes} clientes, {mascotas} mascotas, {veterinarios} veterinarios activos. Consultas hoy: {consultasHoy}, pendientes: {consultasPend}. Ventas este mes: {ventasMes}, ingresos: ${ingresosMes:F2}. Productos en tienda: {productos}. Animales en adopcion: {adopciones}.";
    }

    private async Task<string> ObtenerHistorialMascotaAsync(string preguntaUsuario)
    {
        try
        {
            var mascotas = await _db.Mascotas
                .Where(m => m.Activo)
                .Select(m => new { m.Id, m.Nombre, m.Especie, m.Raza, m.Sexo, m.Peso, m.Edad })
                .ToListAsync();

            foreach (var mascota in mascotas)
            {
                if (preguntaUsuario.Contains(mascota.Nombre.ToLowerInvariant()))
                {
                    var datosBasicos = $"Mascota encontrada: {mascota.Nombre} ({mascota.Especie}, {mascota.Raza ?? "Sin raza"}, {mascota.Sexo}, {mascota.Edad} a\u00f1os, {mascota.Peso}kg).";

                    var consultas = await _db.Consultas
                        .Where(c => c.Activo && c.MascotaId == mascota.Id)
                        .OrderByDescending(c => c.FechaConsulta)
                        .Take(5)
                        .Select(c => new
                        {
                            c.FechaConsulta,
                            c.Motivo,
                            c.Sintomas,
                            c.Diagnostico,
                            c.Estado,
                            Tratamientos = c.Tratamientos.Select(t => new { t.Nombre, t.Medicamento, t.Dosis, t.Frecuencia, t.DuracionDias }).ToList()
                        })
                        .ToListAsync();

                    if (consultas.Count > 0)
                    {
                        var historial = string.Join(" | ", consultas.Select(c =>
                        {
                            var texto = $"[{c.FechaConsulta:dd/MM/yyyy}] Motivo: {c.Motivo}. S\u00edntomas: {c.Sintomas ?? "No registrados"}. Diagn\u00f3stico: {c.Diagnostico ?? "Pendiente"}. Estado: {c.Estado}";
                            if (c.Tratamientos.Any())
                            {
                                var meds = string.Join(", ", c.Tratamientos.Select(t => $"{t.Nombre} ({t.Medicamento ?? "S/M"}, {t.Dosis ?? "S/D"}, {t.Frecuencia ?? "S/F"}, {t.DuracionDias ?? 0}d)"));
                                texto += $" Tratamientos: {meds}";
                            }
                            return texto;
                        }));

                        return $"{datosBasicos} \u00daltimas consultas: {historial}";
                    }

                    return $"{datosBasicos} No hay consultas registradas para esta mascota.";
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al obtener historial de mascota");
        }

        return string.Empty;
    }
}
