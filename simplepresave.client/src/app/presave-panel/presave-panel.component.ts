import { Component } from '@angular/core';
import { SpotifyService } from '../spotify.service';

@Component({
  selector: 'app-presave-panel',
  standalone: true,
  templateUrl: './presave-panel.component.html',
  styleUrl: './presave-panel.component.css'
})

export class PresavePanelComponent {
  constructor(private spotifyService: SpotifyService) { }

  getSpotifyAuthUrl() {
    return this.spotifyService.getAuthUrl();
  }
}
