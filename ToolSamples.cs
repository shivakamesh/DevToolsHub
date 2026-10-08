namespace DevToolsHub;

public static class ToolSamples
{
    public const string JsonSchema = """
        {
          "$schema": "https://json-schema.org/draft/2020-12/schema",
          "type": "object",
          "required": ["id", "email"],
          "properties": {
            "id": { "type": "integer", "minimum": 1 },
            "email": { "type": "string", "format": "email" },
            "tags": { "type": "array", "items": { "type": "string" }, "uniqueItems": true },
            "role": { "enum": ["admin", "user"] }
          },
          "additionalProperties": false
        }
        """;

    public const string JsonSchemaData = """
        {
          "id": 0,
          "email": "not-an-email",
          "tags": ["a", "a"],
          "role": "guest",
          "extra": true
        }
        """;

    public const string JsonDiffLeft = """{ "name": "app", "version": 1, "tags": ["a", "b"], "db": { "host": "localhost", "port": 5432 } }""";

    public const string JsonDiffRight = """{ "name": "app", "version": 2, "tags": ["a", "c"], "db": { "host": "db.local" }, "debug": true }""";

    public const string Svg = """
        <?xml version="1.0" encoding="UTF-8"?>
        <!-- Generator: Inkscape -->
        <svg xmlns="http://www.w3.org/2000/svg" xmlns:inkscape="http://www.inkscape.org/namespaces/inkscape" version="1.1" width="100" height="100" viewBox="0 0 100.000 100.000">
          <metadata>Created with an editor</metadata>
          <g inkscape:label="Layer 1"></g>
          <circle cx="50.123456" cy="50.987654" r="40.5000" fill="#7c5cff" />
          <path d="M10.12345,10.98765 L90.55555,90.44444" stroke="#22d3ee" stroke-width="4.00000" />
        </svg>
        """;

    public const string DockerCompose = """
        services:
          web:
            image: nginx:1.27
            ports:
              - "8080:80"
            environment:
              API_URL: http://api:5000
            depends_on: [api]
          api:
            image: myorg/api:1.0
            deploy:
              replicas: 2
              resources:
                limits:
                  cpus: "0.5"
                  memory: 512M
            expose: ["5000"]
            environment:
              - ConnectionStrings__Db=Host=db;Database=app
            volumes:
              - data:/app/data
        volumes:
          data:
        """;

    public const string OpenApi = """
        openapi: 3.0.3
        info: { title: Pet Store, version: 1.0.0 }
        paths: {}
        components:
          schemas:
            Pet:
              type: object
              required: [id, name]
              properties:
                id: { type: integer, format: int64 }
                name: { type: string, description: The pet's name }
                status: { type: string, enum: [available, pending, sold] }
                tags: { type: array, items: { $ref: '#/components/schemas/Tag' } }
                birthDate: { type: string, format: date }
                owner:
                  type: object
                  properties:
                    email: { type: string, format: email }
            Tag:
              type: object
              properties:
                id: { type: integer }
                label: { type: string, nullable: true }
        """;

    public const string GraphQl = """
        "A user of the system"
        type User implements Node {
          id: ID!
          name: String!
          email: String
          role: Role!
          posts(first: Int = 10): [Post!]!
          createdAt: DateTime!
        }

        interface Node { id: ID! }

        type Post implements Node {
          id: ID!
          title: String!
          tags: [String]
        }

        enum Role { ADMIN EDITOR VIEWER }

        union SearchResult = User | Post

        input CreateUserInput {
          name: String!
          email: String
          role: Role = VIEWER
        }

        scalar DateTime

        type Query {
          user(id: ID!): User
        }
        """;

    public const string Bom = """
        Reference,Qty,Value,MPN,Unit Price
        "R1,R2,R5",3,10k,RC0603FR-0710KL,0.01
        R3,1,4.7k,RC0603FR-074K7L,0.01
        "C1,C2",2,100nF,CL10B104KB8NNNC,0.02
        U1,1,ATmega328P,ATMEGA328P-AU,2.45
        D1,1,LED Red,LTST-C191KRKT,0.12
        R4,1,10k,RC0603FR-0710KL,0.01
        """;
}
