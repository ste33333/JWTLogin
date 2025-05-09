import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthService } from '../../core/auth.service';
import { environment } from '../../../environment/environment';   
import { CommonModule, DatePipe } from '@angular/common';      
import { HttpClient } from '@angular/common/http';             

interface ProtectedApiResponse {
  message: string;
  timestamp: string;
}

@Component({
  selector: 'app-protected-data',
  standalone: true,
  imports: [CommonModule, DatePipe], 
  templateUrl: './protected-data.component.html',
  styleUrls: ['./protected-data.component.css']
})
export class ProtectedDataComponent implements OnInit, OnDestroy {
  apiData: ProtectedApiResponse | null = null;
  apiError: string | null = null;
  isLoading: boolean = false;
  username: string | null = null;


  private dataSub?: Subscription;
  private protectedApiUrl = `${environment.apiUrl}/data/me`;

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.username = this.authService.getCurrentUsername();
    this.loadProtectedData();
  }

  loadProtectedData(): void {
    this.isLoading = true;
    this.apiError = null;
    this.dataSub = this.http.get<ProtectedApiResponse>(this.protectedApiUrl).subscribe({
      next: (response) => {
        this.apiData = response;
        this.isLoading = false;
        console.log('Dati protetti ricevuti:', response);
      },
      error: (err) => {
        this.isLoading = false;
        this.apiError = err.error?.message || err.message || 'Impossibile caricare i dati protetti.';
        console.error('Errore caricamento dati protetti:', err);
        if (err.status === 401) {
            this.apiError = "Non autorizzato. Effettua nuovamente il login.";
        } else if (err.status === 403) {
            this.apiError = "Non hai i permessi per visualizzare questa risorsa.";
        }
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']); 
  }

  ngOnDestroy(): void {
    if (this.dataSub) {
      this.dataSub.unsubscribe();
    }
  }
}