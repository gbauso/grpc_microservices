{{/*
Expand the name of the chart.
*/}}
{{- define "grpc.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "grpc.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "grpc.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "grpc.labels" -}}
helm.sh/chart: {{ include "grpc.chart" . }}
{{ include "grpc.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "grpc.selectorLabels" -}}
app.kubernetes.io/name: {{ include "grpc.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Create the name of the service account to use
*/}}
{{- define "grpc.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
{{- default (include "grpc.fullname" .) .Values.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}

{{- define "grpc.initContainers" -}}
initContainers:
      - name: check-rabbitmq-ready
        image: busybox
        command: [ 'sh', '-c',
            'until wget http://{{ .Values.rabbitmq.user }}:{{ .Values.rabbitmq.password }}@{{ .Values.rabbitmq.host }}:15672/api/aliveness-test/%2F;
            do echo waiting for rabbitmq; sleep 2; done;' ]
      - name: check-elk-ready
        image: busybox
        command: [ 'sh', '-c',
            'until wget http://{{ .Values.elastic.host }}:{{ .Values.elastic.port }};
            do echo waiting for {{ .Values.elastic.host }}; sleep 2; done;' ]
{{- end }}

{{- define "grpc.metrics" -}}
- name: metrics
    targetPort: {{ .Values.metricsPort }} 
    port: {{ .Values.metricsPort }}
{{- end }}