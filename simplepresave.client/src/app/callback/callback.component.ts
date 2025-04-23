import { ActivatedRoute } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { environment } from '../environments/environment';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-callback',
  standalone: false,
  templateUrl: './callback.component.html',
  styleUrl: './callback.component.css'
})
export class CallbackComponent implements OnInit {
  public errorMessage = '';
  public successMessage = '';
  private authCode = '';

  constructor(
    private http: HttpClient,
    private route: ActivatedRoute) { }

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      if (params['code'] && params['state']) {
        this.authCode = params['code'];
        this.getTokens();
      }
      else {
        this.errorMessage = 'No se proporcionó información válida.';
      }
    });
  }

  private getUserTimeOffset(): number {
    // Obtenemos la diferencia horaria del usuario con respecto a UTC+0:00 en minutos, esto con
    // el fin de identificar la medianoche en la zona horaria del usuario el día del lanzamiento.
    const offsetInMinutes = new Date().getTimezoneOffset() * -1;
    return offsetInMinutes;
  }

  private getTokens(): void {
    // Comparamos el state y el redirectUri para asegurarnos de que el flujo es seguro.
    if (!this.validState() || !localStorage.getItem('redirectUri')) {
      this.errorMessage = 'Estado inválido.';
      return;
    }

    const headers = {
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      }
    };
    const body = {
      authorizationCode: this.authCode,
      redirectUri: localStorage.getItem('redirectUri'),
      timeOffset: this.getUserTimeOffset()
    };

    this.http.post(
      `${environment.apiUrl}/token`,
      body,
      headers
    ).subscribe(
      (result) => {
        this.successMessage = '¡Gracias! Tu registro se ha guardado correctamente';
        console.log('Tokens:', result);
      },
      (error) => {
        this.errorMessage = error.error;
        console.error(error);
      }
    );
  }

  private validState(): boolean {
    const storedState = localStorage.getItem('presaveState');
    const currentState = this.route.snapshot.queryParams['state'];
    if (storedState && currentState) {
      return storedState === currentState;
    }
    return false;
  }
}
