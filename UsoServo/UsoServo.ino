/*
 * Autores: Bruna de Barros e Bruno Costa
 * Data: 24/11/2012
 *
 * Uso do Servo Motor
 * Controle do angulo de giro através da porta serial
 * Números devem ser fornecidos através da porta serial no seguinte formato:
 * #<numero>#
 *
 * Exemplo:
 * Posição inicial = #90#
 */

#include <Servo.h>

#define LEFT 0
#define RIGHT 1

Servo servoLeft;
Servo servoRight;

void setup() {
  Serial.begin(9600);
  
  servoLeft.attach(9);
  servoRight.attach(10);
  
  servoLeft.write(0);
  servoRight.write(0);
}

void loop() {
  char c;
  int valor = 0;
  boolean fimNumero = false;
  int motor = -1;
  
  // Tem número pra ler?
  if (Serial.available() > 0) {    
    // Lendo '#'
    c = Serial.read();
    if (c == '#') {
      while (!fimNumero) {
        // Lendo o número
        if (Serial.available() > 0) {
          // Lendo o motor
          c = Serial.read();
          if (c == 'L') {
            motor = LEFT;
          } else if (c == 'R') {
            motor = RIGHT;
          } else if (c >= '0' && c <= '9') {
            valor *= 10;
            valor += c - '0';
          }
          else {
            fimNumero = true;
          }
        }
      }
    }
    
    // Escrevendo o angulo no servo
    if (motor == LEFT) {
      servoLeft.write(valor);
    } else if (motor == RIGHT) {
      servoRight.write(valor);
    }
  }
}

