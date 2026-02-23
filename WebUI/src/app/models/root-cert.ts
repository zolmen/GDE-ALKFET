export interface RootCert {
  id: string;
  subjectName: string;
  notBefore: string;
  notAfter: string;
  thumbprint: string;
  createdAt: string;
  userCertCount: number;
}

export interface CreateRootCert {
  subjectName: string;
  validityDays: number;
}
