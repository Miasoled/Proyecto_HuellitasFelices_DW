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

        public static string PasswordChangedTemplate(string nombreUsuario)
        {
            var content = $@"
            <h2>Contraseña cambiada exitosamente</h2>
            <p>Hola, <strong>{nombreUsuario}</strong>,</p>
            <p>Tu contraseña en <strong>Huellitas Felices</strong> ha sido cambiada correctamente.</p>
            <p>Si realizaste este cambio, no necesitas hacer nada más.</p>
            <p><strong>Si tú no solicitaste este cambio,</strong> por favor contacta a nuestro equipo de soporte inmediatamente en <a href='mailto:info@huellitas.ec'>info@huellitas.ec</a> o llama al (02) 234-5678.</p>
            <p style='margin-top:24px; padding:12px 16px; background:#fef3c7; border-left:4px solid #f59e0b; border-radius:4px; font-size:14px;'>
                <strong>¿No fuiste tú?</strong> Tu cuenta podría estar comprometida. Cambia tu contraseña y revisa tu actividad.
            </p>";

            return BaseTemplate("Contraseña Cambiada - Huellitas Felices", content);
        }

        public static string AccountLockedTemplate(string nombreUsuario)
        {
            var content = $@"
            <h2>Cuenta bloqueada por seguridad</h2>
            <p>Hola, <strong>{nombreUsuario}</strong>,</p>
            <p>Tu cuenta en <strong>Huellitas Felices</strong> ha sido bloqueada temporalmente debido a múltiples intentos de inicio de sesión fallidos.</p>
            <p>El bloqueo durará <strong>15 minutos</strong>. Después de ese tiempo podrás intentar iniciar sesión nuevamente.</p>
            <p><strong>Si no fuiste tú,</strong> te recomendamos cambiar tu contraseña inmediatamente después de desbloquear tu cuenta.</p>
            <p style='margin-top:24px; padding:12px 16px; background:#fef2f2; border-left:4px solid #dc2626; border-radius:4px; font-size:14px;'>
                <strong>⚠️ Alerta de seguridad:</strong> Si no reconoces esta actividad, contacta a soporte.
            </p>";

            return BaseTemplate("Cuenta Bloqueada - Huellitas Felices", content);
        }

        public static string MfaActivatedTemplate(string nombreUsuario)
        {
            var content = $@"
            <h2>Autenticación multifactor activada ✅</h2>
            <p>Hola, <strong>{nombreUsuario}</strong>,</p>
            <p>La autenticación de dos factores (MFA) ha sido activada exitosamente en tu cuenta de <strong>Huellitas Felices</strong>.</p>
            <p>A partir de ahora, al iniciar sesión necesitarás ingresar un código de verificación generado por tu aplicación autenticadora (Google Authenticator, Microsoft Authenticator, etc.).</p>
            <p><strong>Importante:</strong> Guarda tus códigos de recuperación en un lugar seguro. Te permitirán acceder a tu cuenta si pierdes acceso a tu aplicación autenticadora.</p>
            <p style='margin-top:24px; padding:12px 16px; background:#f0fdf4; border-left:4px solid #16a34a; border-radius:4px; font-size:14px;'>
                Tu cuenta ahora tiene una capa adicional de seguridad.
            </p>";

            return BaseTemplate("MFA Activado - Huellitas Felices", content);
        }

        public static string SaleApprovedTemplate(string nombreUsuario, int ventaId, decimal total, string detalle)
        {
            var content = $@"
            <h2>¡Venta confirmada! 🎉</h2>
            <p>Hola, <strong>{nombreUsuario}</strong>,</p>
            <p>Tu venta <strong>#{ventaId}</strong> ha sido procesada exitosamente.</p>
            <div style='background:#f9fafb; border-radius:8px; padding:20px; margin:20px 0; border:1px solid #e5e7eb;'>
                <p style='margin:0 0 8px;'><strong>Número de venta:</strong> #{ventaId}</p>
                <p style='margin:0 0 8px;'><strong>Total:</strong> ${total:F2}</p>
                <p style='margin:0;'><strong>Detalle:</strong> {detalle}</p>
            </div>
            <p>Gracias por tu compra en <strong>Huellitas Felices</strong>.</p>
            <p style='font-size:13px; color:#6b7280;'>Este comprobante sirve como constancia de tu transacción.</p>";

            return BaseTemplate($"Venta #{ventaId} Confirmada - Huellitas Felices", content);
        }

        public static string PaymentFailedTemplate(string nombreUsuario, string razon)
        {
            var content = $@"
            <h2>Pago no procesado</h2>
            <p>Hola, <strong>{nombreUsuario}</strong>,</p>
            <p>Lamentamos informarte que tu pago en <strong>Huellitas Felices</strong> no pudo ser procesado.</p>
            <div style='background:#fef2f2; border-radius:8px; padding:20px; margin:20px 0; border:1px solid #fecaca;'>
                <p style='margin:0 0 8px;'><strong>Motivo:</strong> {razon}</p>
            </div>
            <p>Si el problema persiste, verifica que los datos de tu método de pago sean correctos o intenta con otro medio de pago.</p>
            <p>Si necesitas ayuda, contáctanos en <a href='mailto:info@huellitas.ec'>info@huellitas.ec</a>.</p>";

            return BaseTemplate("Pago No Procesado - Huellitas Felices", content);
        }

        public static string LowStockTemplate(string nombreProducto, int stockActual, int stockMinimo)
        {
            var content = $@"
            <h2>⚠️ Alerta de inventario bajo</h2>
            <p>Se ha detectado un nivel bajo de stock en el siguiente producto:</p>
            <div style='background:#fef3c7; border-radius:8px; padding:20px; margin:20px 0; border:1px solid #fde68a;'>
                <p style='margin:0 0 8px;'><strong>Producto:</strong> {nombreProducto}</p>
                <p style='margin:0 0 8px;'><strong>Stock actual:</strong> {stockActual} unidades</p>
                <p style='margin:0;'><strong>Stock mínimo:</strong> {stockMinimo} unidades</p>
            </div>
            <p>Se recomienda realizar un pedido de reabastecimiento al proveedor correspondiente.</p>
            <p>Accede al sistema para gestionar el inventario.</p>";

            return BaseTemplate("Alerta de Stock Bajo - Huellitas Felices", content);
        }
    }
}
