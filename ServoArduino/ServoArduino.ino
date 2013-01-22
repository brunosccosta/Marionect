#include <Servo.h>

#define DIFF_ANGULO 10

Servo servo;
int angulo;

void setup() {
  Serial.begin(9600);
  
  servo.attach(9);   
  angulo = 0;
  servo.write(angulo);
  
  establishContact();
}

void loop() {
  char inByte;
  
  if (Serial.available() > 0) {
    // get incoming byte:
    inByte = Serial.read();
    
    if (inByte == 'U')
      angulo += DIFF_ANGULO;
    
    if (inByte == 'D')
      angulo -= DIFF_ANGULO;
      
    servo.write(angulo);
  }
}

void establishContact() {
  while (Serial.available() <= 0) {
    Serial.print('A\n');   // send a capital A\n
    delay(300);
  }
}
