UPDATE "Empleados" SET "Activo" = true, "FechaEliminacion" = NULL WHERE "Nombre" = 'Luis Mendoza' AND "Cargo" = 'Veterinario';

SELECT "Id", "Nombre", "Cargo", "Email", "Activo" FROM "Empleados" WHERE "Cargo" = 'Veterinario' AND "Activo" = true ORDER BY "Nombre";
SELECT COUNT(*) as "TotalVets" FROM "Empleados" WHERE "Cargo" = 'Veterinario' AND "Activo" = true;
