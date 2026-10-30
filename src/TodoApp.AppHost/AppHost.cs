var builder = DistributedApplication.CreateBuilder(args);

var usuarioPostgres = builder.AddParameter("usuario-postgres", "postgres");
var senhaPostgres = builder.AddParameter("senha-postgres", "postgres", secret: true);

var postgres = builder.AddPostgres("postgres", usuarioPostgres, senhaPostgres, port: 5432)
    .WithDataVolume("todo-postgres-data")
    .WithPgAdmin();

postgres.AddDatabase("tododb");

builder.AddProject<Projects.TodoApp_Api>("api")
    .WaitFor(postgres);

builder.Build().Run();
