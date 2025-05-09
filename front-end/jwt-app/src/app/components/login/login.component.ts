import { Component } from '@angular/core';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http'; 

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  credentials = { username: '', password: '' };
  errorMessage: string = '';
  isLoading: boolean = false;
  private returnUrl: string = '/dashboard';

  constructor(
    private http: HttpClient,           
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Logica per l'utente già loggato..
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
  }

  onSubmit(): void {
    this.errorMessage = '';
    this.isLoading = true;

    const directLoginUrl = 'https://localhost:7176/api/auth/login'; 
    console.log('LoginComponent: Chiamata DIRETTA a URL:', directLoginUrl);
    console.log('LoginComponent: Credenziali inviate:', this.credentials);

    this.http.post(directLoginUrl, this.credentials).subscribe({ 
      next: (response: any) => {
        this.isLoading = false;
        if (response && response.token) {
          console.log('Login successful (DIRETTO)');
          localStorage.setItem('authToken', response.token); 
          alert('Login con CHIAMATA DIRETTA Riuscito! Token: ' + response.token);
        } else {
          this.errorMessage = `Login fallito (DIRETTO). Token non ricevuto o risposta inattesa. Risposta: ${JSON.stringify(response)}`;
          console.log('LoginComponent (DIRETTO): Risposta senza token:', response);
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = `Errore HTTP (DIRETTO). Status: ${err.status}. URL: ${err.url}. Messaggio: ${err.message || err.error?.message || 'Errore sconosciuto'}`;
        console.error('Login failed in component (DIRETTO)', err);
      }
    });
  }
}