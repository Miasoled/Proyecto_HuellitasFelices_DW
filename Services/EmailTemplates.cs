namespace HuellitasFelices.Services
{
    public static class EmailTemplates
    {
        private static string BaseTemplate(string title, string content)
        {
            return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{title}</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f3f4f6;
            margin: 0;
            padding: 0;
            color: #374151;
        }}
        .container {{
            max-width: 600px;
            margin: 40px auto;
            background-color: #ffffff;
            border-radius: 16px;
            overflow: hidden;
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
        }}
        .header {{
            background: linear-gradient(135deg, #14532d, #16a34a);
            padding: 32px 24px;
            text-align: center;
            color: #ffffff;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
            font-weight: 800;
            letter-spacing: 0.5px;
        }}
        .header p {{
            margin: 8px 0 0;
            font-size: 14px;
            opacity: 0.9;
        }}
        .content {{
            padding: 40px 32px;
            line-height: 1.6;
        }}
        .content p {{
            margin-top: 0;
            margin-bottom: 16px;
            font-size: 16px;
        }}
        .btn {{
            display: inline-block;
            background-color: #16a34a;
            color: #ffffff !important;
            padding: 12px 28px;
            border-radius: 8px;
            text-decoration: none;
            font-weight: 600;
            font-size: 15px;
            margin-top: 16px;
            margin-bottom: 24px;
            text-align: center;
            box-shadow: 0 2px 4px rgba(22, 163, 74, 0.2);
        }}
        .btn:hover {{
            background-color: #15803d;
        }}
        .footer {{
            background-color: #f9fafb;
            padding: 24px 32px;
            text-align: center;
            font-size: 13px;
            color: #6b7280;
            border-top: 1px solid #e5e7eb;
        }}
        .footer a {{
            color: #16a34a;
            text-decoration: none;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🐾 Huellitas Felices</h1>
            <p>Clínica Veterinaria & Centro de Adopciones</p>
        </div>
        <div class='content'>
            {content}
        </div>
        <div class='footer'>
            <p>Este es un correo automático, por favor no respondas a este mensaje.</p>
            <p>&copy; 2026 Huellitas Felices. Av. Amazonas y Naciones Unidas, Quito. Ecuador.</p>
        </div>
    </div>
</body>
</html>";
        }

        public static string WelcomeTemplate(string email, string confirmationLink)
        {
            var content = $@"
            <h2>¡Bienvenido/a a Huellitas Felices!</h2>
            <p>Estamos muy felices de que te unes a nuestra comunidad dedicada al cuidado y bienestar animal.</p>
            <p>Para activar tu cuenta ({email}) y poder agendar citas o solicitar adopciones, por favor confirma tu correo electrónico haciendo clic en el siguiente botón:</p>
            <div style='text-align: center;'>
                <a href='{confirmationLink}' class='btn'>Confirmar Correo Electrónico</a>
            </div>
            <p>Si el botón anterior no funciona, también puedes copiar y pegar el siguiente enlace en tu navegador:</p>
            <p style='font-size: 13px; word-break: break-all; color: #6b7280;'>{confirmationLink}</p>
            <p>¡Gracias por confiar en nosotros!</p>";

            return BaseTemplate("Confirma tu correo electrónico - Huellitas Felices", content);
        }

        public static string ForgotPasswordTemplate(string resetLink)
        {
            var content = $@"
            <h2>Recuperación de Contraseña</h2>
            <p>Hemos recibido una solicitud para restablecer la contraseña de tu cuenta en Huellitas Felices.</p>
            <p>Haz clic en el siguiente botón para establecer una nueva contraseña:</p>
            <div style='text-align: center;'>
                <a href='{resetLink}' class='btn'>Restablecer Contraseña</a>
            </div>
            <p>Este enlace es de un solo uso. Si tú no solicitaste este cambio, puedes ignorar este correo de forma segura; tu contraseña seguirá siendo la misma.</p>
            <p>Si el botón anterior no funciona, copia y pega este enlace en tu navegador:</p>
            <p style='font-size: 13px; word-break: break-all; color: #6b7280;'>{resetLink}</p>";

            return BaseTemplate("Restablecer tu contraseña - Huellitas Felices", content);
        }

        public static string AdoptionTemplate(string applicantName, string animalName, string animalEspecie, string code)
        {
            var content = $@"
            <h2>¡Solicitud de Adopción Recibida! 🐾</h2>
            <p>Hola, <strong>{applicantName}</strong>,</p>
            <p>Hemos recibido correctamente tu solicitud de adopción para <strong>{animalName}</strong> ({animalEspecie}). ¡Muchas gracias por abrir las puertas de tu hogar!</p>
            <p>Nuestros coordinadores de adopciones revisarán tu solicitud con código <strong>#{code}</strong> en un plazo de 48 a 72 horas hábiles y se pondrán en contacto contigo por este medio o por teléfono.</p>
            <p>Queremos asegurarnos de que cada uno de nuestros rescatados encuentre el hogar perfecto para sus necesidades específicas.</p>
            <p>¡Te mantendremos informado sobre el estado de tu solicitud!</p>";

            return BaseTemplate("Solicitud de Adopción Recibida - Huellitas Felices", content);
        }
    }
}
