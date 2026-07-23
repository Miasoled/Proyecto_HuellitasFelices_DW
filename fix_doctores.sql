-- Mantener solo 3 doctores veterinarios
-- Los 3 originales: Dr. Carlos Ramírez, Dra. Ana Torres, Dr. Luis Mendoza
UPDATE "Empleados" SET "Activo" = false, "FechaEliminacion" = NOW()
WHERE "Cargo" = 'Veterinario'
  AND "Nombre" NOT IN ('Dr. Carlos Ramírez', 'Dra. Ana Torres', 'Dr. Luis Mendoza');

-- Verificar
SELECT "Nombre", "Cargo", "Email", "Activo" FROM "Empleados"
WHERE "Cargo" = 'Veterinario' AND "Activo" = true
ORDER BY "Nombre";
SELECT COUNT(*) as "VeterinariosActivos" FROM "Empleados" WHERE "Cargo" = 'Veterinario' AND "Activo" = true;
