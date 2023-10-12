using System;
using System.IO;
using System.Numerics;

// Classe que contém configurações gerais
class Config {
  public static int WIDTH = 100; // Largura da imagem
  public static int HEIGHT = 100; // Altura da imagem
  public static Vector3 red = new Vector3(255, 0, 0); // Cor vermelha
  public static Vector3 green = new Vector3(0, 255, 0); // Cor verde
  public static Vector3 blue = new Vector3(0, 0, 255); // Cor azul
}

// Classe para salvar imagens em formato PPM
class SaveImage {
  public static void Save(string name, string s) {
    // Monta o cabeçalho do arquivo PPM com base nas configurações de Config
    string srt = "P3\n" + Config.WIDTH + " " + Config.HEIGHT + "\n255\n";
    srt += s;
    // Escreve o conteúdo da imagem no arquivo PPM
    File.WriteAllText(name + ".ppm", srt);
  }
}

// Classe que representa uma malha tridimensional
public class Mesh {
  public Vector3[] vertex; // Vetores de vértices
  public Vector3[] normal; // Vetores de normais
  public int[,] tris; // Índices dos triângulos
  public Vector3 color; // Cor da malha
}

// Classe que herda de Mesh para representar uma pirâmide
public class Pyramid : Mesh {
  public Pyramid() {
    // Construtor da pirâmide
    Vector3 p0 = new Vector3(0, 0, 0);
    Vector3 p1 = new Vector3(1, 0, 0);
    Vector3 p2 = new Vector3(0.5f, 0, 1);
    Vector3 p3 = new Vector3(0.5f, 1, 0.5f);
    vertex = new Vector3[4] { p0, p1, p2, p3 }; // Define os vértices da pirâmide
    tris = new int[4, 3] {
      {0, 1, 2},
      {0, 3, 1},
      {1, 3, 2},
      {0, 2, 3}
    }; // Define os triângulos
    normal = new Vector3[4] {
      new Vector3(0, -1, 0), new Vector3(0, -0.45f, 0.9f),
      new Vector3(-0.87f, -0.22f, -0.44f), new Vector3(0.87f, -0.22f, -0.44f)
    }; // Define as normais
    normal = new Vector3[4]; // Limpa as normais
    normal[0] = CalcNormal(vertex[0], vertex[1], vertex[2]); // Calcula a normal da face 0
    normal[1] = CalcNormal(vertex[0], vertex[3], vertex[1]); // Calcula a normal da face 1
    normal[2] = CalcNormal(vertex[1], vertex[3], vertex[2]); // Calcula a normal da face 2
    normal[3] = CalcNormal(vertex[0], vertex[3], vertex[3]); // Calcula a normal da face 3
  }

  // Calcula a normal de uma face da pirâmide com base nos vértices
  Vector3 CalcNormal(Vector3 p0, Vector3 p1, Vector3 p2) {
    Vector3 normal;
    Vector3 v1 = p1 - p0;
    Vector3 v2 = p2 - p0;
    normal = Vector3.Cross(v1, v2); // Produto vetorial para encontrar a normal
    normal = Vector3.Normalize(normal); // Normaliza o vetor
    return normal;
  }
}

// Classe para gerenciar um buffer de pixels e desenhar imagens
public class Buffer {
  public Vector3[,] frame; // Matriz para representar os pixels da imagem

  public Buffer(int w, int h) {
    Config.WIDTH = w; // Define a largura da imagem com base no parâmetro
    Config.HEIGHT = h; // Define a altura da imagem com base no parâmetro
    frame = new Vector3[Config.WIDTH, Config.HEIGHT]; // Inicializa a matriz de pixels
  }

  // Preenche o buffer com uma cor específica
  public void Clear(Vector3 color) {
    for (int h = 0; h < Config.HEIGHT; h++) {
      for (int w = 0; w < Config.WIDTH; w++) {
        frame[w, h] = color;
      }
    }
  }

  // Define a cor de um pixel específico no buffer
  public void SetPixel(int x, int y, Vector3 color) {
    x = Clamp(x, 0, Config.WIDTH - 1); // Garante que
    y = Clamp(y, 0, Config.HEIGHT - 1); // Garante que x e y estão dentro dos limites da imagem
    frame[x, y] = color; // Define a cor do pixel na posição (x, y) no buffer
  }

  // Desenha uma linha entre dois pontos no buffer
  public void DrawLine(Vector3 p1, Vector3 p2, Vector3 color) {
    Vector3 delta = p2 - p1; // Calcula a diferença entre os pontos
    Vector3 ponto = p1;
    float steps = MathF.Max(MathF.Abs(delta.X), MathF.Abs(delta.Y)); // Determina o número de passos
    if (steps != 0)
      delta = delta / steps; // Normaliza o vetor de direção
    for (int i = 0; i < steps; i++) {
      SetPixel((int)ponto.X, (int)ponto.Y, color); // Define a cor do pixel atual
      ponto = ponto + delta; // Move para o próximo pixel na linha
    }
  }

  // Desenha uma malha tridimensional no buffer
  public void DrawMesh(Mesh m) {
    Vector3 camera = new Vector3(0, 0, 1); // Vetor de câmera
    for (int i = 0; i < m.tris.Length / 3; i++) {
      if (Vector3.Dot(m.normal[i], camera) > 0.0) {
        // Verifica a visibilidade da face com base na direção da câmera
        DrawLine(m.vertex[m.tris[i, 0]], m.vertex[m.tris[i, 1]], Config.blue);
        DrawLine(m.vertex[m.tris[i, 1]], m.vertex[m.tris[i, 2]], Config.blue);
        DrawLine(m.vertex[m.tris[i, 2]], m.vertex[0], Config.blue);
      }
    }
  }

  // Garante que um valor está dentro de um intervalo especificado
  int Clamp(int v, int min, int max) {
    return (v < min) ? min : (v > max) ? max : v;
  }

  // Converte o conteúdo do buffer em uma representação de string
  public override string ToString() {
    string s = "";
    for (int h = 0; h < Config.HEIGHT; h++) {
      for (int w = 0; w < Config.WIDTH; w++) {
        s += frame[w, h].X + " " + frame[w, h].Y + " " + frame[w, h].Z + " ";
      }
      s += "\n";
    }
    return s;
  }

  // Salva o conteúdo do buffer em um arquivo de imagem PPM
  public void Save(string name) {
    SaveImage.Save(name, ToString());
  }
}

// Classe que contém funções de transformação geométrica
class Transform {
  public static void Translate(ref Vector3[] p, Vector3 d) {
    for (int i = 0; i < p.Length; i++) {
      p[i] += d; // Aplica uma translação a todos os pontos no vetor
    }
  }

  public static void Scale(ref Vector3[] p, float s) {
    for (int i = 0; i < p.Length; i++) {
      p[i] *= s; // Aplica uma escala a todos os pontos no vetor
    }
  }

  public static void RotateZ(ref Vector3[] p, float a) {
    a *= 0.017452883f; // Converte o ângulo para radianos
    for (int i = 0; i < p.Length; i++) {
      float x = Vector3.Dot(new Vector3(MathF.Cos(a), -MathF.Sin(a), 0), p[i]);
      float y = Vector3.Dot(new Vector3(MathF.Sin(a), MathF.Cos(a), 0), p[i]);
      float z = Vector3.Dot(new Vector3(0, 0, 1), p[i]);
      p[i] = new Vector3(x, y, z); // Rotaciona os pontos em torno do eixo Z
    }
  }

  public static void RotateX(ref Vector3[] p, float a) {
    a *= 0.017452883f; // Converte o ângulo para radianos
    for (int i = 0; i < p.Length; i++) {
      float x = Vector3.Dot(new Vector3(1, 0, 0), p[i]);
      float y = Vector3.Dot(new Vector3(0, MathF.Cos(a), -MathF.Sin(a)), p[i]);
      float z = Vector3.Dot(new Vector3(0, MathF.Sin(a), MathF.Cos(a)), p[i]);
      p[i] = new Vector3(x, y, z); // Rotaciona os pontos em torno do eixo X
    }
  }
}

class Program {
  public static void Main(string[] args) {
    Buffer img = new Buffer(100, 100); // Cria um buffer de imagem
    img.Clear(new Vector3(255, 255, 255)); // Preenche o buffer com uma cor de fundo
    Pyramid p1 = new Pyramid(); // Cria uma pirâmide
    p1.color = new Vector3(0, 0, 255); // Define a cor da pirâmide
    Transform.Scale(ref p1.vertex, 80.0f); // Aplica escala aos vértices da pirâmide
    Transform.Translate(ref p1.vertex, new Vector3(10, 0, 0)); // Aplica translação aos vértices da pirâmide
    for (int i = 0; i < 10; i++) {
      img.Clear(new Vector3(255, 255, 255)); // Limpa o buffer com a cor de fundo
      Transform.Translate(ref p1.vertex, new Vector3(-50, -50, 0)); // Move a pirâmide
      Transform.RotateZ(ref p1.vertex, 5.0f); // Rotaciona a pirâmide em torno do eixo Z
      Transform.RotateZ(ref p1.normal, 5.0f); // Rotaciona as normais em torno do eixo Z
      Transform.RotateX(ref p1.normal, 5.0f); // Rotaciona as normais em torno do eixo X
      Transform.Translate(ref p1.vertex, new Vector3(50, 50, 0)); // Move a pirâmide de volta
      img.DrawMesh(p1); // Desenha a pirâmide no buffer
      img.Save("Imagem" + i); // Salva a imagem em um arquivo
    }
  }
}
