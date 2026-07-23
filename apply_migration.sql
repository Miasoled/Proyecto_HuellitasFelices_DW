-- Create ConsultaMedicamentos table
CREATE TABLE IF NOT EXISTS "ConsultaMedicamentos" (
    "Id" SERIAL PRIMARY KEY,
    "ConsultaId" INTEGER NOT NULL,
    "ProductoId" INTEGER NOT NULL,
    "Cantidad" INTEGER NOT NULL DEFAULT 1,
    "PrecioUnitario" DECIMAL(18,2) NOT NULL,
    "Dosis" VARCHAR(300),
    "Indicaciones" VARCHAR(300),
    CONSTRAINT "FK_ConsultaMedicamentos_Consultas_ConsultaId" FOREIGN KEY ("ConsultaId") REFERENCES "Consultas"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ConsultaMedicamentos_Productos_ProductoId" FOREIGN KEY ("ProductoId") REFERENCES "Productos"("Id") ON DELETE RESTRICT
);
CREATE INDEX IF NOT EXISTS "IX_ConsultaMedicamentos_ConsultaId" ON "ConsultaMedicamentos" ("ConsultaId");
CREATE INDEX IF NOT EXISTS "IX_ConsultaMedicamentos_ProductoId" ON "ConsultaMedicamentos" ("ProductoId");

-- Create Ventas table
CREATE TABLE IF NOT EXISTS "Ventas" (
    "Id" SERIAL PRIMARY KEY,
    "NumeroVenta" VARCHAR(50),
    "ConsultaId" INTEGER NOT NULL UNIQUE,
    "DuenoId" INTEGER,
    "TotalConsulta" DECIMAL(18,2) NOT NULL,
    "TotalMedicamentos" DECIMAL(18,2) NOT NULL,
    "Total" DECIMAL(18,2) GENERATED ALWAYS AS ("TotalConsulta" + "TotalMedicamentos") STORED,
    "Estado" VARCHAR(30) NOT NULL DEFAULT 'Pendiente',
    "MetodoPago" VARCHAR(30),
    "FechaVenta" TIMESTAMP NOT NULL DEFAULT NOW(),
    "FechaPago" TIMESTAMP,
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE,
    "FechaEliminacion" TIMESTAMP,
    "EliminadoPor" VARCHAR(100),
    CONSTRAINT "FK_Ventas_Consultas_ConsultaId" FOREIGN KEY ("ConsultaId") REFERENCES "Consultas"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Ventas_Duenos_DuenoId" FOREIGN KEY ("DuenoId") REFERENCES "Duenos"("Id") ON DELETE SET NULL
);
CREATE INDEX IF NOT EXISTS "IX_Ventas_ConsultaId" ON "Ventas" ("ConsultaId");
CREATE INDEX IF NOT EXISTS "IX_Ventas_DuenoId" ON "Ventas" ("DuenoId");
CREATE INDEX IF NOT EXISTS "IX_Ventas_Estado" ON "Ventas" ("Estado");

-- Create DetallesVenta table
CREATE TABLE IF NOT EXISTS "DetallesVenta" (
    "Id" SERIAL PRIMARY KEY,
    "VentaId" INTEGER NOT NULL,
    "ProductoId" INTEGER NOT NULL,
    "Cantidad" INTEGER NOT NULL,
    "PrecioUnitario" DECIMAL(18,2) NOT NULL,
    "Subtotal" DECIMAL(18,2) GENERATED ALWAYS AS ("Cantidad" * "PrecioUnitario") STORED,
    CONSTRAINT "FK_DetallesVenta_Ventas_VentaId" FOREIGN KEY ("VentaId") REFERENCES "Ventas"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_DetallesVenta_Productos_ProductoId" FOREIGN KEY ("ProductoId") REFERENCES "Productos"("Id") ON DELETE RESTRICT
);
CREATE INDEX IF NOT EXISTS "IX_DetallesVenta_VentaId" ON "DetallesVenta" ("VentaId");
CREATE INDEX IF NOT EXISTS "IX_DetallesVenta_ProductoId" ON "DetallesVenta" ("ProductoId");
