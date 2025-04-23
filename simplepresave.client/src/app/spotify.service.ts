import { Injectable } from '@angular/core';
import { environment } from './environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SpotifyService {
  public clientId: string = environment.spotifyClientId;
  private response_type: string = 'code';
  private redirect_uri: string = `${environment.landingPageUrl}/callback`;
  private state: string;
  private scope: string = environment.spotifyScope;

  constructor() {
    
    // Generar un valor aleatorio para el state y lo almacenamos localmente
    this.state = Math.random().toString(36).substring(2, 18);
    localStorage.setItem('presaveState', this.state);
    localStorage.setItem('redirectUri', this.redirect_uri);
  }

  getAuthUrl(): string {
    const baseUrl = 'https://accounts.spotify.com/authorize';
    const queryParams = new URLSearchParams({
      client_id: this.clientId,
      response_type: this.response_type,
      state: this.state,
      scope: this.scope,
      redirect_uri: this.redirect_uri
    });

    return `${baseUrl}?${queryParams.toString()}`;
  }
}
